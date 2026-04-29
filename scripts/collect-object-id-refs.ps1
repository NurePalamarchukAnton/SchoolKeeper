# Collects references to entity IDs across SchoolKeeper (web) and SchoolKeeperAndroid.
# Excludes wwwroot/lib, node_modules, *.min.js
# Usage: .\scripts\collect-object-id-refs.ps1 [-RepoRoot <path>]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"
$outPath = Join-Path $RepoRoot "docs\object-id-audit.txt"
$dirs = @(
    (Join-Path $RepoRoot "SchoolKeeper"),
    (Join-Path $RepoRoot "SchoolKeeperAndroid")
) | Where-Object { Test-Path $_ }

function Get-SourceFiles {
    param([string[]]$Paths)
    foreach ($p in $Paths) {
        Get-ChildItem -Path $p -Recurse -File -Include *.cs, *.kt, *.cshtml, *.js -ErrorAction SilentlyContinue |
            Where-Object {
                $_.FullName -notmatch '[\\/]wwwroot[\\/]lib[\\/]' -and
                $_.FullName -notmatch '[\\/]node_modules[\\/]' -and
                $_.FullName -notmatch '\\build\\' -and
                $_.FullName -notmatch '\\.gradle\\' -and
                $_.Name -notmatch '\.min\.js$'
            }
    }
}

function Add-Section {
    param(
        [System.Text.StringBuilder]$Sb,
        [string]$Title,
        [string]$Pattern,
        [System.IO.FileInfo[]]$Files
    )
    [void]$Sb.AppendLine("")
    [void]$Sb.AppendLine("=== $Title ===")
    [void]$Sb.AppendLine("Pattern: $Pattern")
    [void]$Sb.AppendLine("")
    $count = 0
    foreach ($f in $Files) {
        try {
            $hits = Select-String -Path $f.FullName -Pattern $Pattern -AllMatches -ErrorAction SilentlyContinue
            foreach ($h in $hits) {
                [void]$Sb.AppendLine("$($f.FullName.Replace($RepoRoot, '.').TrimStart('.\')):$($h.LineNumber):$($h.Line.Trim())")
                $count++
            }
        } catch { }
    }
    [void]$Sb.AppendLine("")
    [void]$Sb.AppendLine("# Matches in section: $count")
}

$files = @(Get-SourceFiles -Paths $dirs)
$sb = [System.Text.StringBuilder]::new()

$branch = try { git -C $RepoRoot rev-parse --abbrev-ref HEAD 2>$null } catch { "unknown" }
if (-not $branch) { $branch = "unknown" }

[void]$sb.AppendLine("object-id-audit.txt")
[void]$sb.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')")
[void]$sb.AppendLine("Git branch: $branch")
[void]$sb.AppendLine("Repo root: $RepoRoot")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("Excluded: **/wwwroot/lib/**, **/node_modules/**, **/build/**, **/.gradle/**, *.min.js")
[void]$sb.AppendLine("Method: PowerShell Select-String (rg not required). Re-run: .\scripts\collect-object-id-refs.ps1")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("=== Triage notes (short) ===")
[void]$sb.AppendLine("- DbSeeder.cs: test seed data uses relational links (school.Id, etc.), not arbitrary magic IDs.")
[void]$sb.AppendLine("- Android AdminDataScreen: fallback ?: 1 for default school/user when lists empty - review UX.")
[void]$sb.AppendLine("- RegisterScreen: schoolId default ?: 1 if parse fails.")
[void]$sb.AppendLine("- Many matches are DTO property names (schoolId field) - normal; literals/fallbacks need closer review.")
[void]$sb.AppendLine("")

# Section 1: entity-related field names
Add-Section -Sb $sb -Title "Field names (schoolId, userId, deviceId, ...)" -Pattern "schoolId|userId|deviceId|incidentId|reportedBy|generatedBy|originalAdminId|SchoolId|UserId|DeviceId|IncidentId" -Files $files

# Section 2: elvis / fallback numeric (common hardcoded defaults)
Add-Section -Sb $sb -Title "Elvis and fallbacks (?: N)" -Pattern "\?\s*:\s*\d+" -Files $files

# Section 3: sentinel id == 0 for create
Add-Section -Sb $sb -Title "Sentinel new-record (id == 0)" -Pattern "(id\s*==\s*0|Id\s*==\s*0|\.id\s*==\s*0)" -Files $files

# Section 4: route template {id} (informational)
Add-Section -Sb $sb -Title "Route placeholders (curly id)" -Pattern "\{id\}" -Files $files

$null = New-Item -ItemType Directory -Force -Path (Split-Path $outPath -Parent)
[System.IO.File]::WriteAllText($outPath, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
Write-Host "Wrote $outPath ($($sb.Length) chars)"
