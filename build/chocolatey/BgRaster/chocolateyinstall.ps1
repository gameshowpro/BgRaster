# BgRaster Chocolatey install script
# BgRaster is a self-contained AOT-compiled executable. No runtime dependencies needed.

$ErrorActionPreference = 'Stop'

$packageName = $env:ChocolateyPackageName
$toolsDir = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)"
$binariesDir = Join-Path (Split-Path -Parent $toolsDir) 'binaries'

# Copy binaries to tools directory so Chocolatey can create shims
$files = Get-ChildItem -Path $binariesDir -File
foreach ($file in $files) {
    $destination = Join-Path $toolsDir $file.Name
    if (Test-Path $destination) {
        Remove-Item $destination -Force
    }
    Copy-Item -Path $file.FullName -Destination $toolsDir -Force
    Write-Host "Installed: $($file.Name)"
}

Write-Host "BgRaster installed successfully. Run 'BgRaster --help' to get started."

Write-Host ""
Write-Host "BgRaster participates in the Open Source Maintenance Fee. Organizations that"
Write-Host "use it as part of revenue-generating activities pay a small monthly fee that"
Write-Host "funds maintenance. Who pays and how: https://bgraster.gameshow.pro/maintenance-fee/"
