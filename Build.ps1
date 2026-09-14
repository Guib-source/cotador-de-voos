<# Compila o aplicativo com o compilador .NET Framework incluído no Windows. #>
param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$versionText = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'VersionInfo.cs'))
$version = [regex]::Match($versionText, 'Display = "([^"]+)"').Groups[1].Value
if (!$version) { throw 'Versão ausente em VersionInfo.cs.' }
if (!$OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot ('bin/' + $version) }
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (!(Test-Path -LiteralPath $compiler)) { throw 'Instale o .NET Framework 4.x para compilar.' }
$sources = @(Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | ForEach-Object { $_.FullName })
& $compiler /nologo /target:winexe "/out:$OutputDirectory\Cotador.exe" "/win32icon:$PSScriptRoot\assets\Cotador.ico" "/resource:$PSScriptRoot\assets\Cotador.ico,Cotador.ico" "/resource:$PSScriptRoot\assets\Cotador.png,Cotador.png" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll $sources
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação. Feche o aplicativo se o executável estiver em uso.' }
if ($OutputDirectory.TrimEnd('\') -ne $PSScriptRoot.TrimEnd('\')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Ocr.ps1') -Destination $OutputDirectory -Force
}
Write-Output ('Compilação concluída: ' + (Join-Path $OutputDirectory 'Cotador.exe'))
