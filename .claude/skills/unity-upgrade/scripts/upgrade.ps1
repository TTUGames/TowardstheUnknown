# Moves the project to another Unity editor version (a new Unity 7 alpha): installs it if needed, closes the open
# editor, opens the project once in batch mode on a new branch chore/unity-<version>, fails on C# or shader errors,
# then summarizes what the upgrade re-serialized. Run from the repo root on Windows, in PowerShell 7:
#   pwsh .claude/skills/unity-upgrade/scripts/upgrade.ps1 -Version 7000.0.0a17
param([Parameter(Mandatory)][string]$Version, [switch]$KeepLibrary)
$env:NO_COLOR = 1; $env:UNITY_NO_BANNER = 1
$root = (git rev-parse --show-toplevel).Trim()
Set-Location $root
if (git status --porcelain) { "The working tree has changes: commit or stash them first."; exit 2 }
$current = ((Get-Content ProjectSettings/ProjectVersion.txt | Select-String '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value).Trim()
if ($current -eq $Version) { "Already on $Version."; exit 0 }

$editor = "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe"
if (-not (Test-Path $editor)) {
  "Installing $Version with Windows IL2CPP..."
  unity install $Version -m windows-il2cpp --no-cm -y --non-interactive
  if (-not (Test-Path $editor)) { "Install failed: $editor is missing."; exit 1 }
}

unity close $root --timeout 120 2>&1 | Out-Null
git switch -c "chore/unity-$Version"
# A clean import catches what an incremental one would hide (the Library of the old version is rebuilt either way)
if (-not $KeepLibrary) { Remove-Item Library, Temp, obj -Recurse -Force -ErrorAction SilentlyContinue }

New-Item -ItemType Directory -Force Logs | Out-Null
$log = Join-Path $root "Logs/upgrade-$Version.log"
"Opening the project in $Version (batch mode, a few minutes)..."
$sw = [Diagnostics.Stopwatch]::StartNew()
$p = Start-Process $editor -ArgumentList "-batchmode", "-quit", "-projectPath", "`"$root`"", "-logFile", "`"$log`"" -Wait -PassThru
"Done in $([int]$sw.Elapsed.TotalSeconds) s, exit code $($p.ExitCode), log $log"

$errors = Select-String -Path $log -Pattern "error CS\d+|Scripts have compiler errors|Shader error in" |
  ForEach-Object { $_.Line.Trim() } | Sort-Object -Unique
if ($errors) { "Errors:"; $errors | Select-Object -First 30; exit 1 }

"ProjectVersion: " + ((Get-Content ProjectSettings/ProjectVersion.txt)[0])
"Re-serialized by the upgrade, by folder:"
git status --porcelain | ForEach-Object { ($_.Substring(3).Trim('"') -split '/')[0..1] -join '/' } |
  Group-Object | Sort-Object Count -Descending | ForEach-Object { "{0,5}  {1}" -f $_.Count, $_.Name }
"Packages changed:"
git diff Packages/manifest.json | Select-String '^[-+] ' | ForEach-Object { "  " + $_.Line.Trim() }
