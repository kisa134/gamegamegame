# PreToolUse gate on `git commit`.
# The core gates are law (CLAUDE.md section 4). A red commit never gets created,
# so a red branch never reaches a PR. Runs only when the staged set touches the
# .NET core, so docs-only commits stay instant.

$ErrorActionPreference = 'Stop'

function Send-Decision {
    param([string]$Decision, [string]$Reason)
    @{
        hookSpecificOutput = @{
            hookEventName            = 'PreToolUse'
            permissionDecision       = $Decision
            permissionDecisionReason = $Reason
        }
    } | ConvertTo-Json -Depth 5 -Compress
    exit 0
}

try {
    $raw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }
    $cmd = ($raw | ConvertFrom-Json).tool_input.command
} catch { exit 0 }

if ([string]::IsNullOrWhiteSpace($cmd)) { exit 0 }
if ($cmd -notmatch 'git\s+commit\b') { exit 0 }

$repo = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Push-Location $repo
try {
    $staged = & git diff --cached --name-only 2>$null
    if (-not ($staged -match '^(src|tests)/')) { exit 0 }

    $testOutput = & dotnet test Game.sln --configuration Release --nologo 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) {
        $tail = ($testOutput -split "`n" | Select-Object -Last 15) -join "`n"
        Send-Decision 'deny' "Core gate failed: dotnet test is red, so this commit is blocked (CLAUDE.md section 4). Fix the tests first.`n$tail"
    }

    $audit = & git grep -nE 'using[[:space:]]+(UnityEngine|UnityEditor|TMPro)' -- src/Game.Domain src/Game.Application 2>$null
    if ($audit) {
        Send-Decision 'deny' "Dependency audit failed: Unity usings found in Domain/Application, which the core must never reference (CLAUDE.md section 3).`n$audit"
    }
} finally {
    Pop-Location
}

exit 0
