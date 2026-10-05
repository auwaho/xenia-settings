$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path "$PSScriptRoot/..").Path
$outputDirectory = Join-Path $PSScriptRoot 'bin/Release/cases/release-workflow'
$checks = 0
function Check([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
    $script:checks++
}

$archivePath = & "$projectRoot/.github/scripts/package-release.ps1" -OutputDirectory $outputDirectory
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($archivePath)
try {
    $names = @($zip.Entries.FullName | Sort-Object)
    Check (($names -join ',') -eq 'LICENSE,xenia_settings.exe') 'Package contains only the application and license.'
    foreach ($name in $names) {
        $source = if ($name -eq 'LICENSE') { Join-Path $projectRoot $name } else { Join-Path $projectRoot "XeniaSettings/bin/Release/$name" }
        $stream = $zip.GetEntry($name).Open()
        $memory = New-Object IO.MemoryStream
        try {
            $stream.CopyTo($memory)
            Check ([Convert]::ToBase64String($memory.ToArray()) -eq [Convert]::ToBase64String([IO.File]::ReadAllBytes($source))) "Packaged $name matches the build output."
        }
        finally { $stream.Dispose(); $memory.Dispose() }
    }
}
finally { $zip.Dispose() }

$buildWithoutConfig = Join-Path $outputDirectory 'exe-only'
New-Item -ItemType Directory -Path $buildWithoutConfig -Force | Out-Null
Copy-Item -LiteralPath "$projectRoot/XeniaSettings/bin/Release/xenia_settings.exe" -Destination $buildWithoutConfig -Force
$archiveWithoutConfig = & "$projectRoot/.github/scripts/package-release.ps1" -BuildDirectory $buildWithoutConfig -OutputDirectory "$outputDirectory/no-config"
Check ((Test-Path -LiteralPath $archiveWithoutConfig) -and -not (Test-Path "$buildWithoutConfig/xenia_settings.exe.config")) 'Packaging succeeds without the optional EXE config file.'

$failed = $false
try { & "$projectRoot/.github/scripts/package-release.ps1" -BuildDirectory "$outputDirectory/missing" -OutputDirectory "$outputDirectory/invalid" }
catch { $failed = $true }
Check ($failed -and -not (Test-Path "$outputDirectory/invalid/xenia-settings-windows.zip")) 'Missing application files fail packaging before creating an archive.'

# Stub only the external CLI; exercise the real publisher without network calls or tokens.
function gh {
    $command = [string[]]$args
    $global:xeniaReleaseTestState.Calls.Add($command)
    $global:LASTEXITCODE = 0
    if ($command[0] -eq 'api' -and $command[1].EndsWith('/commits/main')) {
        return $global:xeniaReleaseTestState.Head
    }
    if ($command[0] -eq 'release' -and $command[1] -eq 'view') {
        if ($global:xeniaReleaseTestState.Scenario -eq 'new') { $global:LASTEXITCODE = 1; return }
        return ($global:xeniaReleaseTestState.Scenario -eq 'draft').ToString().ToLowerInvariant()
    }
    if ($command[0] -eq 'release' -and $command[1] -eq 'upload' -and $global:xeniaReleaseTestState.Scenario -eq 'upload-failure') {
        $global:LASTEXITCODE = 1
    }
}

$commit = '0123456789abcdef0123456789abcdef01234567'
try {
    foreach ($scenario in @('new', 'published', 'draft', 'upload-failure', 'stale')) {
        $global:xeniaReleaseTestState = @{
            Scenario = $scenario
            Head = if ($scenario -eq 'stale') { 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' } else { $commit }
            Calls = New-Object 'System.Collections.Generic.List[object]'
        }
        $failed = $false
        try { & "$projectRoot/.github/scripts/publish-release.ps1" -Repository 'example/xenia-settings' -Commit $commit -Archive $archivePath }
        catch { $failed = $true }
        $calls = $global:xeniaReleaseTestState.Calls
        $publishCalls = @($calls | Where-Object { $_ -contains '--draft=false' })
        $tagCalls = @($calls | Where-Object { $_ -contains 'PATCH' })
        if ($scenario -eq 'stale') {
            Check (-not $failed -and $calls.Count -eq 1) 'An older completed workflow cannot mutate the newer release.'
        }
        elseif ($scenario -eq 'upload-failure') {
            Check ($failed -and $publishCalls.Count -eq 0 -and $tagCalls.Count -eq 0) 'A failed asset upload stops before moving the tag or publishing the release.'
            Check (@($calls | Where-Object { $_ -contains '--draft=true' }).Count -eq 1) 'An incomplete refreshed release remains a draft for recovery.'
        }
        else {
            Check (-not $failed -and $publishCalls.Count -eq 1 -and $tagCalls.Count -eq 1) "$scenario release publishes after uploading and moving the tag."
            Check ($tagCalls[0] -contains "sha=$commit" -and $publishCalls[0] -contains $commit) "$scenario release points to the tested commit."
            $uploadCalls = @($calls | Where-Object { $_[0] -eq 'release' -and $_[1] -eq 'upload' })
            Check ($uploadCalls.Count -eq 1 -and $uploadCalls[0] -contains $archivePath -and $uploadCalls[0] -contains '--clobber') "$scenario release uploads the tested archive under the same asset name."
            if ($scenario -ne 'new') {
                Check (@($calls | Where-Object { $_[0] -eq 'release' -and $_[1] -eq 'create' }).Count -eq 0) "$scenario release reuses the existing release."
            }
        }
    }
}
finally { Remove-Variable -Name xeniaReleaseTestState -Scope Global -ErrorAction SilentlyContinue }
Write-Host "PASS: $checks release workflow checks."
