[CmdletBinding()]
param(
    [string]$ProjectPath = "src/BgRaster.csproj",
    [string]$SampleConfigDirectory = "docs/sample-config",
    [string]$SampleOutputDirectory = "docs/sample-output"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    if (-not (Test-Path $SampleConfigDirectory)) {
        throw "Sample config directory not found: $SampleConfigDirectory"
    }

    New-Item -ItemType Directory -Path $SampleOutputDirectory -Force | Out-Null

    $configs = Get-ChildItem -Path $SampleConfigDirectory -Filter *.toml | Sort-Object Name
    if ($configs.Count -eq 0) {
        Write-Warning "No sample config files found in $SampleConfigDirectory"
        return
    }

    foreach ($config in $configs) {
        $stem = [System.IO.Path]::GetFileNameWithoutExtension($config.Name)
        $renderTemplate = Join-Path $SampleOutputDirectory $stem

        Write-Host "Generating sample: $($config.Name) -> $renderTemplate.png"

        $dotnetArgs = @(
            "run"
            "--project"
            $ProjectPath
            "--"
            "--config"
            $config.FullName
            "--no-discovery"
            "true"
            "--no-assignment"
            "true"
            "--render-output"
            $renderTemplate
        )

        & dotnet @dotnetArgs

        if ($LASTEXITCODE -ne 0) {
            throw "BgRaster failed for sample config '$($config.Name)' with exit code $LASTEXITCODE"
        }
    }
}
finally {
    Pop-Location
}
