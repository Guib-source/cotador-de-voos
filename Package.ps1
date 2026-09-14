<# Prepara um pacote local; não cria tag, não mescla branches e não publica release. #>
param()
$ErrorActionPreference = 'Stop'
$version = [regex]::Match([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'VersionInfo.cs')), 'Display = "([^"]+)"').Groups[1].Value
if (!$version) { throw 'Versão ausente.' }
$package = Join-Path $PSScriptRoot ('releases/Cotador-de-Voos-Windows-v' + $version + '.zip')
if (Test-Path -LiteralPath $package) { throw 'Este pacote já existe. Incremente a versão antes de preparar outra entrega.' }
& (Join-Path $PSScriptRoot 'Build.ps1')
& (Join-Path $PSScriptRoot 'tests/Test.ps1')
& (Join-Path $PSScriptRoot 'tests/Test-Visual.ps1')
$binary = Join-Path $PSScriptRoot ('bin/' + $version + '/Cotador.exe')
if ([Diagnostics.FileVersionInfo]::GetVersionInfo($binary).ProductVersion -ne $version) { throw 'A versão do executável diverge do código-fonte.' }
New-Item -ItemType Directory -Path (Split-Path -Parent $package) -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
$zip = [IO.Compression.ZipFile]::Open($package, [IO.Compression.ZipArchiveMode]::Create)
try {
    $files = @(Get-ChildItem -LiteralPath $PSScriptRoot -File | Where-Object { !$_.Name.StartsWith('.') -and $_.Extension -notin @('.exe','.pdb') })
    $files += @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'tests') -File)
    $files += @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'tests/fixtures') -File)
    $files += @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'assets') -File)
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($PSScriptRoot.Length + 1).Replace('\','/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, ('Cotador/' + $relative)) | Out-Null
    }
    [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $binary, 'Cotador/Cotador.exe') | Out-Null
} finally { $zip.Dispose() }
Write-Output ('Pacote local preparado: ' + $package)
