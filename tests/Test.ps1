# Testes independentes dos prints originais e de pacotes externos.
$ErrorActionPreference = 'Stop'
$appDirectory = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$testDirectory = Join-Path ([IO.Path]::GetTempPath()) ('cotador-tests-' + [Guid]::NewGuid())
New-Item -ItemType Directory -Path $testDirectory | Out-Null
try {
    Copy-Item -LiteralPath "$appDirectory\Cotador.exe" -Destination "$testDirectory\Cotador.exe"
    & $compiler /nologo /target:exe "/out:$testDirectory\Tests.exe" "/reference:$appDirectory\Cotador.exe" "$PSScriptRoot\Regression.cs"
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar testes.' }
    & "$testDirectory\Tests.exe" "$PSScriptRoot\fixtures"
    if ($LASTEXITCODE -ne 0) { throw 'Falha nos testes.' }
} finally {
    # Somente os dois arquivos criados neste diretório temporário são removidos.
    Remove-Item -LiteralPath "$testDirectory\Tests.exe", "$testDirectory\Cotador.exe" -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $testDirectory -ErrorAction SilentlyContinue
}
