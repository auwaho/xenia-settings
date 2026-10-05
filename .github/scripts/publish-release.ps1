param(
    [Parameter(Mandatory = $true)]
    [string]$Repository,
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9a-fA-F]{40}$')]
    [string]$Commit,
    [string]$Archive = "$PSScriptRoot/../../dist/xenia-settings-windows.zip"
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$archivePath = (Resolve-Path -LiteralPath $Archive).Path
$tag = 'latest-build'

function Invoke-GitHub([string[]]$Arguments) {
    $output = & gh @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "GitHub CLI failed: gh $($Arguments -join ' ')`n$($output -join [Environment]::NewLine)" }
    $output
}

# A slower older workflow must not replace the build for a newer push to main.
$mainCommit = Invoke-GitHub -Arguments @('api', "repos/$Repository/commits/main", '--jq', '.sha')
if ($mainCommit -ne $Commit) {
    Write-Host 'Skipping release: main has a newer commit.'
    return
}

$notes = @"
Automatically built and tested from commit [$Commit](https://github.com/$Repository/commit/$Commit).

Download and extract xenia-settings-windows.zip. Copy xenia_settings.exe into your Xenia Canary folder, keeping your existing emulator config and patches.

Requires Windows and .NET Framework 4.7.2 or later. This release is updated after successful builds of main.
"@
$notesPath = Join-Path (Split-Path -Parent $archivePath) 'release-notes.md'
Set-Content -LiteralPath $notesPath -Value $notes -Encoding utf8

& gh release view $tag --repo $Repository --json isDraft --jq '.isDraft' >$null 2>$null
if ($LASTEXITCODE -eq 0) {
    # Keep a partially refreshed archive private if uploading or moving the tag fails.
    Invoke-GitHub -Arguments @('release', 'edit', $tag, '--repo', $Repository, '--draft=true')
}
else {
    Invoke-GitHub -Arguments @('release', 'create', $tag, '--repo', $Repository,
        '--target', $Commit, '--draft', '--title', 'Latest Build', '--notes-file', $notesPath)
}

Invoke-GitHub -Arguments @('release', 'upload', $tag, $archivePath, '--repo', $Repository, '--clobber')
# A draft release does not create its tag until publication; first builds and retries may lack it.
& gh api "repos/$Repository/git/ref/tags/$tag" >$null 2>$null
if ($LASTEXITCODE -eq 0) {
    Invoke-GitHub -Arguments @('api', '--method', 'PATCH', "repos/$Repository/git/refs/tags/$tag",
        '-f', "sha=$Commit", '-F', 'force=true')
}
else {
    Invoke-GitHub -Arguments @('api', '--method', 'POST', "repos/$Repository/git/refs",
        '-f', "ref=refs/tags/$tag", '-f', "sha=$Commit")
}
Invoke-GitHub -Arguments @('release', 'edit', $tag, '--repo', $Repository,
    '--target', $Commit, '--title', 'Latest Build', '--notes-file', $notesPath, '--draft=false', '--latest')
