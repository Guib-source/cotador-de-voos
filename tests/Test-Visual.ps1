# Renderiza a interface sem abrir janelas. Saída de QA não entra no pacote.
$ErrorActionPreference = 'Stop'
$appDirectory = Split-Path -Parent $PSScriptRoot
$outputDirectory = Join-Path $PSScriptRoot 'visual-output'
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
Copy-Item -LiteralPath "$appDirectory\Cotador.exe" -Destination "$outputDirectory\Cotador.exe"
try {
    & $compiler /nologo /target:exe "/out:$outputDirectory\VisualTests.exe" "/reference:$outputDirectory\Cotador.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "$PSScriptRoot\VisualRegression.cs"
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar teste visual.' }
    & "$outputDirectory\VisualTests.exe" $outputDirectory
    if ($LASTEXITCODE -ne 0) { throw 'Falha no teste visual.' }
} finally {
    Remove-Item -LiteralPath "$outputDirectory\VisualTests.exe", "$outputDirectory\Cotador.exe" -ErrorAction SilentlyContinue
}
