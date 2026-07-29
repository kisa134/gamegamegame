# PreToolUse guard for Bash commands.
# Encodes the repo rules that cost us real incidents, so no agent can repeat them.
# Reads the hook payload on stdin, emits a PreToolUse permission decision on stdout.

$ErrorActionPreference = 'Stop'

function Send-Decision {
    param([string]$Decision, [string]$Reason)
    $out = @{
        hookSpecificOutput = @{
            hookEventName            = 'PreToolUse'
            permissionDecision       = $Decision
            permissionDecisionReason = $Reason
        }
    }
    $out | ConvertTo-Json -Depth 5 -Compress
    exit 0
}

try {
    $raw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }
    $cmd = ($raw | ConvertFrom-Json).tool_input.command
} catch {
    # A guard that cannot parse must not block real work.
    exit 0
}
if ([string]::IsNullOrWhiteSpace($cmd)) { exit 0 }

# 1. Blanket staging. MYgame/ is a 2.9 GB Unity project; Assets/ and ProjectSettings/
#    were unignored once already. Stage by explicit path instead.
if ($cmd -match '(^|[;&|]\s*)git\s+add\s+(-A\b|--all\b|\.(\s|$))') {
    Send-Decision 'deny' 'Blanket staging is forbidden in this repo: git add -A / git add . would sweep the MYgame/ Unity project and other untracked files into the commit. Stage by explicit path: git add src/... tests/...'
}

# 2. main is merge-only. CLAUDE.md section 5: work lands through a PR, never a direct push.
if ($cmd -match 'git\s+push\b[^;&|]*\borigin\s+(main\b|HEAD:main\b)') {
    Send-Decision 'deny' 'Direct push to main is forbidden (CLAUDE.md section 5). Push a feature/jira-SCRUM-* branch and open a PR instead.'
}

# 3. Merging is the Principal's decision, not the agent's. Ask, do not silently block:
#    Yura authorizes merges, and when he does the command must still be runnable.
if ($cmd -match 'gh\s+pr\s+merge\b') {
    Send-Decision 'ask' 'Merging into main is the Principal''s call (CLAUDE.md section 5). Confirm only if Yura has authorized this merge in the conversation.'
}

exit 0
