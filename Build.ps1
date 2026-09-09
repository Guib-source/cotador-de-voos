<# Compila o aplicativo com o compilador .NET Framework incluído no Windows. #>
param()
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (!(Test-Path -LiteralPath $compiler)) { throw 'Instale o .NET Framework 4.x para compilar.' }
$sources = @(Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | ForEach-Object { $_.FullName })
& $compiler /nologo /target:winexe "/out:$PSScriptRoot\Cotador.exe" "/win32icon:$PSScriptRoot\assets\Cotador.ico" "/resource:$PSScriptRoot\assets\Cotador.ico,Cotador.ico" "/resource:$PSScriptRoot\assets\Cotador.png,Cotador.png" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll $sources
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação. Feche o aplicativo se o executável estiver em uso.' }
Write-Output 'Compilação concluída: Cotador.exe'
