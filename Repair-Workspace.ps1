$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[Console]::InputEncoding = $utf8NoBom
[Console]::OutputEncoding = $utf8NoBom
$OutputEncoding = $utf8NoBom

Set-Location $PSScriptRoot

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)
    & git @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

function Invoke-DotNet {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$secretPath = Join-Path $PSScriptRoot 'Secret.cs'
$secretBackup = Join-Path $env:TEMP "Meshok-Secret-$stamp.cs"

if (Test-Path $secretPath) {
    Copy-Item $secretPath $secretBackup -Force
}

Invoke-Git -Arguments @('config', 'core.quotepath', 'false')

# Clear index flags that can leave old physical files behind while status looks clean.
$trackedFiles = @(& git -c core.quotepath=false ls-files)
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to enumerate tracked files.'
}
foreach ($file in $trackedFiles) {
    if (-not [string]::IsNullOrWhiteSpace($file)) {
        & git update-index --no-assume-unchanged --no-skip-worktree -- $file
        if ($LASTEXITCODE -ne 0) {
            throw "Unable to clear index flags for $file"
        }
    }
}

$changes = @(& git status --porcelain=v1 -uall)
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to read repository status.'
}
if ($changes.Count -gt 0) {
    Invoke-Git -Arguments @('stash', 'push', '-u', '-m', "workspace-repair-$stamp")
}

Invoke-Git -Arguments @('fetch', 'origin', 'main')
Invoke-Git -Arguments @('reset', '--hard', 'origin/main')
Invoke-Git -Arguments @('clean', '-fd')

if (Test-Path $secretBackup) {
    Copy-Item $secretBackup $secretPath -Force
    Remove-Item $secretBackup -Force
}

# Remove only generated build state. User sources and the ignored Secret.cs stay intact.
Get-ChildItem $PSScriptRoot -Directory -Recurse -Force |
    Where-Object {
        $_.Name -in @('bin', 'obj') -and
        $_.FullName -notmatch '[\\/]\.git[\\/]'
    } |
    Sort-Object { $_.FullName.Length } -Descending |
    Remove-Item -Recurse -Force

$vsPath = Join-Path $PSScriptRoot '.vs'
if (Test-Path $vsPath) {
    try {
        Remove-Item $vsPath -Recurse -Force -ErrorAction Stop
    }
    catch {
        Write-Warning "Unable to remove locked Visual Studio cache '$vsPath'. Build will continue. Close Visual Studio to remove it later."
    }
}

Invoke-DotNet -Arguments @('clean', 'Meshok.Sln')
Invoke-DotNet -Arguments @('restore', 'Meshok.Sln')
Invoke-DotNet -Arguments @('build', 'Meshok.Sln')

$legacyBrowserHits = @(
    Get-ChildItem @('GH.Components', 'MehokBrowser', 'NewsMaker') -Filter '*.cs' -Recurse |
        Select-String -Pattern 'using Gecko|GeckoWebBrowser|GeckoDocument|GeckoHtmlElement|Xpcom'
)
if ($legacyBrowserHits.Count -gt 0) {
    throw "Legacy Gecko code is still present in $($legacyBrowserHits.Count) source locations."
}

Write-Host "Workspace repaired at commit: $(& git log -1 --oneline)" -ForegroundColor Green
Write-Host 'Secret.cs was preserved. Previous local changes remain safely in git stash.' -ForegroundColor Green
