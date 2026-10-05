param(
    [string]$BuildDirectory = "$PSScriptRoot/../../XeniaSettings/bin/Release",
    [string]$OutputDirectory = "$PSScriptRoot/../../dist"
)

$ErrorActionPreference = 'Stop'
$files = @(
    (Join-Path $BuildDirectory 'xenia_settings.exe'),
    "$PSScriptRoot/../../LICENSE"
)
# Explicit files keep the sample emulator config and patches out of the release.
$files = @($files | ForEach-Object { (Resolve-Path -LiteralPath $_).Path })
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$archive = Join-Path $OutputDirectory 'xenia-settings-windows.zip'
Compress-Archive -LiteralPath $files -DestinationPath $archive -Force
Write-Output (Get-Item -LiteralPath $archive).FullName
