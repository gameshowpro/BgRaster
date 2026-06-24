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