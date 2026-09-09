# Ícone vetorial original, sem fontes ou imagens externas. Gera ICO multirresolução.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$iconSizes = @(16, 24, 32, 48, 64, 128, 256)
$frames = @()
foreach ($iconSize in $iconSizes) {
    $bitmap = New-Object Drawing.Bitmap($iconSize, $iconSize)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([Drawing.Color]::Transparent)
    $graphics.ScaleTransform(($iconSize / 256.0), ($iconSize / 256.0))
    $tile = New-Object Drawing.Drawing2D.GraphicsPath
    $tile.AddArc(8, 8, 108, 108, 180, 90)
    $tile.AddArc(140, 8, 108, 108, 270, 90)
    $tile.AddArc(140, 140, 108, 108, 0, 90)
    $tile.AddArc(8, 140, 108, 108, 90, 90)
    $tile.CloseFigure()
    $teal = New-Object Drawing.SolidBrush([Drawing.Color]::FromArgb(0,119,110))
    $graphics.FillPath($teal, $tile)
    $graphics.TranslateTransform(128,128)
    $graphics.RotateTransform(35)
    $graphics.TranslateTransform(-128,-128)
    $coordinates = @(128,34,140,60,140,102,203,149,203,169,140,146,140,185,160,203,160,217,128,205,96,217,96,203,116,185,116,146,53,169,53,149,116,102,116,60)
    $points = @()
    for ($index = 0; $index -lt $coordinates.Count; $index += 2) {
        $points += New-Object Drawing.PointF($coordinates[$index], $coordinates[$index + 1])
    }
    $graphics.FillPolygon([Drawing.Brushes]::White, [Drawing.PointF[]]$points)
    $stream = New-Object IO.MemoryStream
    $bitmap.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
    $frames += ,($stream.ToArray())
    if ($iconSize -eq 256) { $bitmap.Save((Join-Path $PSScriptRoot 'Cotador.png'), [Drawing.Imaging.ImageFormat]::Png) }
    $stream.Dispose(); $graphics.Dispose(); $bitmap.Dispose(); $tile.Dispose(); $teal.Dispose()
}
$output = [IO.File]::Create((Join-Path $PSScriptRoot 'Cotador.ico'))
$writer = New-Object IO.BinaryWriter($output)
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$iconSizes.Count)
    $offset = 6 + 16 * $iconSizes.Count
    for ($index = 0; $index -lt $iconSizes.Count; $index++) {
        $dimension = $iconSizes[$index] % 256
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
        $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32)
        $writer.Write([uint32]$frames[$index].Length); $writer.Write([uint32]$offset)
        $offset += $frames[$index].Length
    }
    foreach ($frame in $frames) { $writer.Write([byte[]]$frame) }
} finally { $writer.Dispose(); $output.Dispose() }
Write-Output 'Ícone gerado: 16, 24, 32, 48, 64, 128 e 256 px.'
