param(
    [string]$ProjectPath = "src/BgRaster.csproj",
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$PublishDir = "",
    [switch]$SkipPublish
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

if ([string]::IsNullOrWhiteSpace($PublishDir)) {
    $PublishDir = Join-Path $repoRoot "publish/$Runtime"
}

$resolvedPublishDir = [System.IO.Path]::GetFullPath($PublishDir)

if (-not $SkipPublish) {
    Write-Host "Publishing AOT binary to '$resolvedPublishDir'..."
    dotnet publish $ProjectPath -c $Configuration -r $Runtime /p:PublishAot=true /p:TreatWarningsAsErrors=true -o $resolvedPublishDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

$publishedExecutables = @(Get-ChildItem -LiteralPath $resolvedPublishDir -Filter "*.exe" -File)
if ($publishedExecutables.Count -eq 0) {
    throw "No published executable was found in '$resolvedPublishDir'."
}
if ($publishedExecutables.Count -gt 1) {
    $exeNames = ($publishedExecutables.Name | Sort-Object) -join ", "
    throw "Multiple executables were found in '$resolvedPublishDir': $exeNames"
}

$exePath = $publishedExecutables[0].FullName

Write-Host "Running published --help smoke check..."
& $exePath --help | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Published binary --help failed with exit code $LASTEXITCODE"
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("bgraster-smoke-" + [System.Guid]::NewGuid().ToString("N"))
$outputDir = Join-Path $tempRoot "out"
$configPath = Join-Path $tempRoot "config.toml"

New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

$outputTemplate = Join-Path $outputDir "wall_{index}"
$escapedOutputTemplate = $outputTemplate.Replace("\", "\\")

$configToml = @"
[text]
text = ["AOT Smoke"]
size = ["4vh"]

[logo]
source = ["pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg"]

[render]
no-discovery = true
no-assignment = true
output = "$escapedOutputTemplate"
verbosity = "quiet"

[[output]]
target = 0

[output.hardware_output]
id = "SMOKE-0"
index = 0
desktopX = 0
desktopY = 0
widthPx = 640
heightPx = 360
dpiX = 96
dpiY = 96
rotation = 0
friendlyName = "SmokeDisplay"
adapterName = "SmokeAdapter"
"@

Set-Content -LiteralPath $configPath -Value $configToml -Encoding UTF8

Write-Host "Running published render smoke check..."
& $exePath --config $configPath | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Published binary render smoke check failed with exit code $LASTEXITCODE"
}

$expectedOutput = Join-Path $outputDir "wall_0.png"
if (-not (Test-Path -LiteralPath $expectedOutput)) {
    throw "Smoke render did not produce expected output file '$expectedOutput'."
}

Write-Host "Smoke test passed. Output file: $expectedOutput"