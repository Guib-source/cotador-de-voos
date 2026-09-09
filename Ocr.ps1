param([string]$ImagePath, [string]$OutputPath, [ValidateRange(1,5)][double]$ScaleFactor=3)
$ErrorActionPreference = 'Stop'
# O Windows PowerShell 5.1 fornece a ponte WinRT usada pelo OCR local.
Add-Type -AssemblyName System.Runtime.WindowsRuntime
[Windows.Storage.StorageFile, Windows.Storage, ContentType=WindowsRuntime] | Out-Null
[Windows.Graphics.Imaging.BitmapDecoder, Windows.Foundation, ContentType=WindowsRuntime] | Out-Null
[Windows.Media.Ocr.OcrEngine, Windows.Foundation, ContentType=WindowsRuntime] | Out-Null
$script:asTask = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object { $_.Name -eq 'AsTask' -and $_.IsGenericMethod -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1' } | Select-Object -First 1
function Await($Operation, $Type) {
    # Operações WinRT genéricas não podem ser aguardadas diretamente pelo PowerShell.
    $task = $script:asTask.MakeGenericMethod($Type).Invoke($null, @($Operation))
    $task.Wait()
    return $task.Result
}
$original = $null
$scaled = $null
$graphics = $null
$bitmap = $null
$stream = $null
$tempImage = $null
try {
    Add-Type -AssemblyName System.Drawing
    $original = [Drawing.Image]::FromFile($ImagePath)
    # Amplie os caracteres pequenos sem ultrapassar a dimensão suportada pelo OCR.
    $scale = [Math]::Min($ScaleFactor, 3800.0 / [Math]::Max($original.Width, $original.Height))
    $scaled = New-Object Drawing.Bitmap ([int]($original.Width * $scale)), ([int]($original.Height * $scale))
    $graphics = [Drawing.Graphics]::FromImage($scaled)
    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.DrawImage($original, 0, 0, $scaled.Width, $scaled.Height)
    $tempImage = [IO.Path]::Combine([IO.Path]::GetTempPath(), [Guid]::NewGuid().ToString() + '.png')
    $scaled.Save($tempImage, [Drawing.Imaging.ImageFormat]::Png)
    $ImagePath = $tempImage
    $file = Await ([Windows.Storage.StorageFile]::GetFileFromPathAsync($ImagePath)) ([Windows.Storage.StorageFile])
    $stream = Await ($file.OpenAsync([Windows.Storage.FileAccessMode]::Read)) ([Windows.Storage.Streams.IRandomAccessStream])
    $decoder = Await ([Windows.Graphics.Imaging.BitmapDecoder]::CreateAsync($stream)) ([Windows.Graphics.Imaging.BitmapDecoder])
    $bitmap = Await ($decoder.GetSoftwareBitmapAsync()) ([Windows.Graphics.Imaging.SoftwareBitmap])
    $engine = [Windows.Media.Ocr.OcrEngine]::TryCreateFromUserProfileLanguages()
    if (!$engine) { throw 'Nenhum idioma de OCR está instalado no Windows.' }
    $result = Await ($engine.RecognizeAsync($bitmap)) ([Windows.Media.Ocr.OcrResult])
    # O OCR pode devolver colunas em vez de linhas da tabela. Reconstrua a ordem
    # pelas coordenadas: primeiro altura, depois posição horizontal em cada linha.
    $words = @($result.Lines | ForEach-Object { $_.Words } | Sort-Object { $_.BoundingRect.Y })
    $rows = New-Object 'System.Collections.Generic.List[string]'
    $row = @()
    $rowY = -100.0
    $rowTolerancePixels = 18 # Tolerância calibrada para as ampliações 2x e 3x testadas.
    foreach ($word in $words) {
        if ($word.BoundingRect.Y - $rowY -gt $rowTolerancePixels -and $row.Count -gt 0) {
            $rows.Add((($row | Sort-Object { $_.BoundingRect.X } | ForEach-Object { $_.Text }) -join ' '))
            $row = @()
        }
        if ($row.Count -eq 0) { $rowY = $word.BoundingRect.Y }
        $row += $word
    }
    if ($row.Count) { $rows.Add((($row | Sort-Object { $_.BoundingRect.X } | ForEach-Object { $_.Text }) -join ' ')) }
    $text = $rows -join "`r`n"
    [IO.File]::WriteAllText($OutputPath, $text, [Text.Encoding]::UTF8)
} catch {
    [IO.File]::WriteAllText($OutputPath, ('ERRO: ' + $_.Exception.Message), [Text.Encoding]::UTF8)
    exit 1
} finally {
    # Execute também em falhas para não manter a imagem bloqueada ou arquivos órfãos.
    foreach ($resource in @($bitmap, $stream, $graphics, $scaled, $original)) {
        if ($null -ne $resource) { $resource.Dispose() }
    }
    if ($tempImage -and (Test-Path -LiteralPath $tempImage)) {
        Remove-Item -LiteralPath $tempImage -Force
    }
}
