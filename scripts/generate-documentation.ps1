[CmdletBinding()]
param(
    [string]$ProjectPath = "src/BgRaster.csproj",
    [string]$SampleConfigDirectory = "docs/sample-config",
    [string]$SampleOutputDirectory = "docs/generated"
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

    $schemaPath = Join-Path $repoRoot "docs/schemas/bgraster-config.schema.json"
    if (-not (Test-Path $schemaPath)) {
        throw "Schema file not found: $schemaPath"
    }

    $schema = Get-Content -Raw -Path $schemaPath | ConvertFrom-Json -Depth 100

    function Write-GeneratedMarkdownFile {
        param(
            [string]$FileName,
            [string]$Content
        )

        $destination = Join-Path $SampleOutputDirectory $FileName
        [System.IO.File]::WriteAllText($destination, $Content + "`n", [System.Text.UTF8Encoding]::new($false))
    }

    function Sync-BrandingAssets {
        $sourceLogoPath = Join-Path $repoRoot "resources/gsp.svg"
        if (-not (Test-Path $sourceLogoPath)) {
            throw "Branding source file not found: $sourceLogoPath"
        }

        $docsImageDirectory = Join-Path $repoRoot "docs/assets/images"
        New-Item -ItemType Directory -Path $docsImageDirectory -Force | Out-Null

        foreach ($fileName in @("logo.svg", "favicon.svg")) {
            $destinationPath = Join-Path $docsImageDirectory $fileName
            Copy-Item -Path $sourceLogoPath -Destination $destinationPath -Force
        }
    }

    function ConvertTo-CliOptionsTable {
        param([object]$Schema)

        $cliOptionsProperty = $Schema.PSObject.Properties['x-cli-options']
        if ($null -eq $cliOptionsProperty -or $null -eq $cliOptionsProperty.Value) {
            $existingCliTablePath = Join-Path $SampleOutputDirectory "cli-schema.md"
            if (Test-Path $existingCliTablePath) {
                Write-Warning "Schema metadata 'x-cli-options' is missing in docs/schemas/bgraster-config.schema.json. Reusing existing generated/cli-schema.md."
                return (Get-Content -Raw -Path $existingCliTablePath).TrimEnd("`r", "`n")
            }

            Write-Warning "Schema metadata 'x-cli-options' is missing in docs/schemas/bgraster-config.schema.json and no existing generated/cli-schema.md was found. Emitting a placeholder table."
            return @(
                '| Option | Type | TOML equivalent | Description | Default resolution |',
                '|---|---|---|---|---|',
                '| _Missing schema metadata_ | - | - | Add `x-cli-options` to `docs/schemas/bgraster-config.schema.json`. | - |'
            ) -join "`n"
        }

        $lines = @(
            '| Option | Type | TOML equivalent | Description | Default resolution |',
            '|---|---|---|---|---|'
        )

        foreach ($option in $cliOptionsProperty.Value) {
            $optionSyntax = if ([string]::IsNullOrWhiteSpace([string]$option.valueSyntax)) {
                [string]$option.alias
            }
            else {
                '{0} {1}' -f $option.alias, $option.valueSyntax
            }

            $lines += ('| `{0}` | `{1}` | `{2}` | {3} | {4} |' -f $optionSyntax, $option.typeName, $option.tomlEquivalent, $option.description, $option.defaultResolution)
        }

        return $lines -join "`n"
    }

    function ConvertTo-TomlRootScalarsTable {
        param([object]$Schema)

        $lines = @(
            '| Key | Type | Default | Description |',
            '|---|---|---|---|'
        )

        foreach ($entry in $Schema.properties.PSObject.Properties) {
            $value = $entry.Value
            if ($value.PSObject.Properties.Name -contains '$ref') {
                continue
            }

            if ($value.type -in @('object', 'array')) {
                continue
            }

            $defaultValue = ($value.default | ConvertTo-Json -Compress)
            $lines += ('| `{0}` | `{1}` | `{2}` | {3} |' -f $entry.Name, ($value.type ?? '-'), $defaultValue, $value.description)
        }

        return $lines -join "`n"
    }

    Write-GeneratedMarkdownFile -FileName "cli-schema.md" -Content (ConvertTo-CliOptionsTable -Schema $schema)
    Write-GeneratedMarkdownFile -FileName "toml-root-scalars.md" -Content (ConvertTo-TomlRootScalarsTable -Schema $schema)
    Sync-BrandingAssets

    # Generate docs/index.md from README.md, rewriting repo-root-relative paths to docs/-relative paths.
    $readmePath = Join-Path $repoRoot "README.md"
    $indexContent = Get-Content -Raw -Path $readmePath
    # Strip the docs/ prefix from markdown links and image references so paths resolve correctly in MkDocs.
    $indexContent = $indexContent -replace '\]\(docs/', ']('
    $docsIndexPath = Join-Path $repoRoot "docs/index.md"
    [System.IO.File]::WriteAllText($docsIndexPath, "<!-- This file is generated by scripts/generate-documentation.ps1 from README.md. Do not edit directly. -->`n`n" + $indexContent, [System.Text.UTF8Encoding]::new($false))
    Write-Host "Generated docs/index.md from README.md"

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
