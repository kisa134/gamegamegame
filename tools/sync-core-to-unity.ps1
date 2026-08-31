# Builds the core and drops the netstandard2.1 assemblies into the Unity project.
#
# Unity never compiles the core's source: it consumes IL. That is what keeps the core's
# language version, implicit usings and nullable settings independent of the engine,
# which is the point of CLAUDE.md section 3. Run this whenever src/ changes; the Unity
# layer itself needs no rebuild.
#
# ASCII only: Windows PowerShell 5.1 reads a BOM-less .ps1 as ANSI and chokes on any
# non-ASCII byte.

$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$dest = Join-Path $repo 'MYgame\Assets\Plugins\GameCore'

Write-Host 'Building core (net8.0 + netstandard2.1)...'
& dotnet build (Join-Path $repo 'Game.sln') -c Release --nologo
if ($LASTEXITCODE -ne 0) { throw 'Core build failed; nothing copied.' }

if (-not (Test-Path $dest)) { New-Item -ItemType Directory -Force $dest | Out-Null }

$assemblies = 'Game.Domain', 'Game.Application'
foreach ($name in $assemblies) {
    $from = Join-Path $repo "src\$name\bin\Release\netstandard2.1"
    # DLLs only. A .pdb next to them makes Unity generate an extra .meta on every sync,
    # and player stack traces are not what a playtest needs.
    $file = Join-Path $from "$name.dll"
    if (-not (Test-Path $file)) {
        throw "Missing $file - did the netstandard2.1 target build?"
    }

    Copy-Item $file $dest -Force
    Write-Host "  copied $name.dll"
}

Write-Host "Core synced to $dest"
