# watch-docs.ps1 - polls for changes and regenerates docs.
# Runs generate-documentation.ps1 whenever schema, sample config, README,
# branding assets, or the doc generator script itself change.

param(
    [string]$RepoRoot = $PSScriptRoot
)

$watchPaths = @(
    'docs/schemas',
    'docs/sample-config',
    'README.md',
    'scripts/generate-documentation.ps1',
    'resources'
)

$marker = [System.IO.Path]::GetTempFileName()
try {
    [System.IO.File]::SetLastWriteTimeUtc($marker, [System.DateTime]::UtcNow)
    Write-Host "[watch-docs] Watching for changes..."

    while ($true) {
        $changed = $null
        foreach ($watchPath in $watchPaths) {
            $fullPath = Join-Path $RepoRoot $watchPath
            if (Test-Path $fullPath -PathType Container) {
                $found = Get-ChildItem -Path $fullPath -Recurse -File -ErrorAction SilentlyContinue |
                    Where-Object { $_.LastWriteTimeUtc -gt [System.IO.File]::GetLastWriteTimeUtc($marker) } |
                    Select-Object -First 1
                if ($found) { $changed = $found.FullName; break }
            }
            elseif (Test-Path $fullPath -PathType Leaf) {
                $item = Get-Item $fullPath -ErrorAction SilentlyContinue
                if ($item -and $item.LastWriteTimeUtc -gt [System.IO.File]::GetLastWriteTimeUtc($marker)) {
                    $changed = $item.FullName; break
                }
            }
        }

        if ($changed) {
            [System.IO.File]::SetLastWriteTimeUtc($marker, [System.DateTime]::UtcNow)
            Write-Host "[watch-docs] Change detected in $changed, regenerating..."
            & "$RepoRoot/scripts/generate-documentation.ps1" -RepoRoot $RepoRoot
            Write-Host "[watch-docs] Regeneration complete."
        }

        Start-Sleep -Seconds 2
    }
}
finally {
    if (Test-Path $marker) { Remove-Item $marker -Force }
}