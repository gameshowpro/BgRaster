[CmdletBinding()]
param(
    [string]$RepoRoot,
    [string]$ProjectPath = "src/BgRaster.csproj",
    [string]$SampleConfigDirectory = "docs/sample-config",
    [string]$SampleOutputDirectory = "docs/generated"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = if ($RepoRoot) { $RepoRoot } else { Split-Path -Parent $PSScriptRoot }

# Resolve all relative-path parameters to absolute so UNC working directory quirks don't break them.
$ProjectPath = if ([System.IO.Path]::IsPathRooted($ProjectPath)) { $ProjectPath } else { Join-Path $repoRoot $ProjectPath }
$SampleConfigDirectory = if ([System.IO.Path]::IsPathRooted($SampleConfigDirectory)) { $SampleConfigDirectory } else { Join-Path $repoRoot $SampleConfigDirectory }
$SampleOutputDirectory = if ([System.IO.Path]::IsPathRooted($SampleOutputDirectory)) { $SampleOutputDirectory } else { Join-Path $repoRoot $SampleOutputDirectory }

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

    $commonSchemaPath = Join-Path $repoRoot "docs/schemas/bgraster-common.schema.json"
    if (-not (Test-Path $commonSchemaPath)) {
        throw "Common schema file not found: $commonSchemaPath"
    }

    $schema = Get-Content -Raw -Path $schemaPath | ConvertFrom-Json -Depth 100
    $commonSchema = Get-Content -Raw -Path $commonSchemaPath | ConvertFrom-Json -Depth 100

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
        param(
            [object]$Schema,
            [object]$CommonSchema
        )

        $options = @()
        $bgrasterProperty = $Schema.PSObject.Properties['x-bgraster']
        if ($null -ne $bgrasterProperty -and $null -ne $bgrasterProperty.Value) {
            if ($null -ne $bgrasterProperty.Value.cliOptions) {
                $options += @($bgrasterProperty.Value.cliOptions)
            }

            if ($null -ne $bgrasterProperty.Value.cliOnlyOptions) {
                $options += @($bgrasterProperty.Value.cliOnlyOptions)
            }
        }

        if ($options.Count -eq 0) {
            throw "Schema metadata for CLI options is missing in docs/schemas/bgraster-config.schema.json (x-bgraster.cliOptions / x-bgraster.cliOnlyOptions)."
        }

        $lines = @(
            '| Option | Type | TOML equivalent | Description | Default resolution |',
            '|---|---|---|---|---|'
        )

        foreach ($option in $options) {
            $optionSyntax = if ([string]::IsNullOrWhiteSpace([string]$option.valueSyntax)) {
                [string]$option.alias
            }
            else {
                '{0} {1}' -f $option.alias, $option.valueSyntax
            }

            $tomlEquivalent = if ([string]::IsNullOrWhiteSpace([string]$option.tomlEquivalent)) {
                if ([string]::IsNullOrWhiteSpace([string]$option.tomlPath)) { '-' } else { [string]$option.tomlPath }
            }
            else {
                [string]$option.tomlEquivalent
            }

            $typeName = if ([string]::IsNullOrWhiteSpace([string]$option.typeName)) { '-' } else { [string]$option.typeName }
            $description = if ([string]::IsNullOrWhiteSpace([string]$option.description)) { '-' } else { [string]$option.description }
            $tomlPath = [string]$option.tomlPath
            if (-not [string]::IsNullOrWhiteSpace($tomlPath)) {
                $pathNode = Resolve-TomlPathSchemaNode -TomlPath $tomlPath -ConfigSchema $Schema -CommonSchema $CommonSchema
                $enumSuffix = Get-EnumDescriptionSuffix -PrimaryNode $pathNode -ResolvedNode $pathNode
                if (-not [string]::IsNullOrWhiteSpace($enumSuffix)) {
                    $description = ('{0}{1}' -f $description, $enumSuffix)
                }
            }

            $defaultResolution = if ([string]::IsNullOrWhiteSpace([string]$option.defaultResolution)) { '-' } else { [string]$option.defaultResolution }

            $lines += ('| `{0}` | `{1}` | `{2}` | {3} | {4} |' -f $optionSyntax, $typeName, $tomlEquivalent, $description, $defaultResolution)
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

    function Get-SchemaNodeByPointer {
        param(
            [object]$Root,
            [string]$Pointer
        )

        if ([string]::IsNullOrWhiteSpace($Pointer) -or $Pointer -eq '#') {
            return $Root
        }

        if (-not $Pointer.StartsWith('#', [StringComparison]::Ordinal)) {
            throw "Unsupported JSON pointer '$Pointer'."
        }

        $path = $Pointer.Substring(1)
        if ($path.StartsWith('/', [StringComparison]::Ordinal)) {
            $path = $path.Substring(1)
        }

        $current = $Root
        foreach ($rawSegment in ($path -split '/')) {
            if ([string]::IsNullOrWhiteSpace($rawSegment)) {
                continue
            }

            $segment = $rawSegment.Replace('~1', '/').Replace('~0', '~')
            $property = $current.PSObject.Properties[$segment]
            if ($null -eq $property) {
                throw "JSON pointer segment '$segment' not found in '$Pointer'."
            }

            $current = $property.Value
        }

        return $current
    }

    function Resolve-SchemaRefNode {
        param(
            [string]$Ref,
            [object]$ConfigSchema,
            [object]$CommonSchema
        )

        if ([string]::IsNullOrWhiteSpace($Ref)) {
            throw "Schema ref is required."
        }

        if ($Ref.StartsWith('./bgraster-common.schema.json#', [StringComparison]::Ordinal)) {
            return Get-SchemaNodeByPointer -Root $CommonSchema -Pointer ($Ref.Substring('./bgraster-common.schema.json'.Length))
        }

        if ($Ref.StartsWith('#', [StringComparison]::Ordinal)) {
            try {
                return Get-SchemaNodeByPointer -Root $ConfigSchema -Pointer $Ref
            }
            catch {
                return Get-SchemaNodeByPointer -Root $CommonSchema -Pointer $Ref
            }
        }

        throw "Unsupported schema ref '$Ref'."
    }

    function Resolve-TomlPathSchemaNode {
        param(
            [string]$TomlPath,
            [object]$ConfigSchema,
            [object]$CommonSchema
        )

        if ([string]::IsNullOrWhiteSpace($TomlPath) -or $TomlPath -eq '-') {
            return $null
        }

        $normalized = $TomlPath.Replace('[', '').Replace(']', '')
        $segments = $normalized.Split('.', [System.StringSplitOptions]::RemoveEmptyEntries)
        if ($segments.Count -eq 0) {
            return $null
        }

        $current = $ConfigSchema
        foreach ($segment in $segments) {
            if ($null -eq $current) {
                return $null
            }

            if ($current.PSObject.Properties.Name -contains '$ref') {
                $current = Resolve-SchemaRefNode -Ref ([string]$current.'$ref') -ConfigSchema $ConfigSchema -CommonSchema $CommonSchema
            }

            $properties = Get-OptionalPropertyValue -Node $current -PropertyName 'properties'
            if ($null -eq $properties) {
                return $null
            }

            $nextProperty = $properties.PSObject.Properties[$segment]
            if ($null -eq $nextProperty) {
                return $null
            }

            $current = $nextProperty.Value
        }

        if ($null -ne $current -and $current.PSObject.Properties.Name -contains '$ref') {
            $current = Resolve-SchemaRefNode -Ref ([string]$current.'$ref') -ConfigSchema $ConfigSchema -CommonSchema $CommonSchema
        }

        return $current
    }

    function Get-EnumValues {
        param([object]$Node)

        if ($null -eq $Node) {
            return @()
        }

        if ($Node.PSObject.Properties.Name -contains 'enum' -and $null -ne $Node.enum) {
            return @($Node.enum)
        }

        if ($Node.PSObject.Properties.Name -contains 'type' -and $Node.type -eq 'array') {
            $itemsNode = Get-OptionalPropertyValue -Node $Node -PropertyName 'items'
            if ($null -ne $itemsNode -and $itemsNode.PSObject.Properties.Name -contains 'enum' -and $null -ne $itemsNode.enum) {
                return @($itemsNode.enum)
            }
        }

        if ($Node.PSObject.Properties.Name -contains 'oneOf' -and $null -ne $Node.oneOf) {
            $values = @()
            foreach ($candidate in $Node.oneOf) {
                if ($candidate.PSObject.Properties.Name -contains 'enum' -and $null -ne $candidate.enum) {
                    $values += @($candidate.enum)
                }
            }

            if ($values.Count -gt 0) {
                return $values
            }
        }

        return @()
    }

    function ConvertTo-EnumLiteral {
        param([object]$Value)

        if ($null -eq $Value) {
            return '`null`'
        }

        if ($Value -is [string]) {
            return ('`{0}`' -f $Value)
        }

        if ($Value -is [bool]) {
            return ('`{0}`' -f $Value.ToString().ToLowerInvariant())
        }

        return ('`{0}`' -f ([string]$Value))
    }

    function Get-EnumDescriptionSuffix {
        param(
            [object]$PrimaryNode,
            [object]$ResolvedNode
        )

        $enumValues = @(Get-EnumValues -Node $PrimaryNode)
        if ($enumValues.Count -eq 0 -and $ResolvedNode -ne $PrimaryNode) {
            $enumValues = @(Get-EnumValues -Node $ResolvedNode)
        }

        if ($enumValues.Count -eq 0) {
            return ''
        }

        $formattedValues = @()
        foreach ($enumValue in ($enumValues | Select-Object -Unique)) {
            $formattedValues += (ConvertTo-EnumLiteral -Value $enumValue)
        }

        return (' Allowed values: {0}.' -f ($formattedValues -join ', '))
    }

    function Get-SchemaTypeName {
        param([object]$Node)

        if ($null -eq $Node) { return '-' }

        if ($Node.PSObject.Properties.Name -contains 'oneOf') {
            $types = @()
            foreach ($item in $Node.oneOf) {
                if ($item.PSObject.Properties.Name -contains 'type') {
                    $types += [string]$item.type
                }
                elseif ($item.PSObject.Properties.Name -contains '$ref') {
                    $types += 'object'
                }
            }

            if ($types.Count -eq 0) { return 'oneOf' }
            return (($types | Select-Object -Unique) -join '|')
        }

        if (($Node.PSObject.Properties.Name -contains 'type') -and $Node.type -eq 'array') {
            $itemType = if ($Node.PSObject.Properties.Name -contains 'items') {
                Get-SchemaTypeName -Node $Node.items
            }
            else {
                'object'
            }

            return "${itemType}[]"
        }

        if ($Node.PSObject.Properties.Name -contains 'type') {
            return [string]$Node.type
        }

        if ($Node.PSObject.Properties.Name -contains '$ref') {
            return 'object'
        }

        return '-'
    }

    function Get-DefaultLiteral {
        param([object]$Value)

        if ($null -eq $Value) { return '-' }
        return ($Value | ConvertTo-Json -Compress)
    }

    function Escape-MarkdownCell {
        param([string]$Value)

        if ([string]::IsNullOrWhiteSpace($Value)) { return '-' }
        return $Value.Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
    }

    function Get-OptionalPropertyValue {
        param(
            [object]$Node,
            [string]$PropertyName
        )

        if ($null -eq $Node) { return $null }
        $property = $Node.PSObject.Properties[$PropertyName]
        if ($null -eq $property) { return $null }
        return $property.Value
    }

    function ConvertTo-TomlSectionsMarkdown {
        param(
            [object]$ConfigSchema,
            [object]$CommonSchema
        )

        $xBgrasterProperty = $ConfigSchema.PSObject.Properties['x-bgraster']
        $xBgraster = if ($null -eq $xBgrasterProperty) { $null } else { $xBgrasterProperty.Value }
        $reference = if ($null -eq $xBgraster) { $null } else { $xBgraster.tomlReference }
        if ($null -eq $reference) {
            throw "Schema metadata x-bgraster.tomlReference is missing in docs/schemas/bgraster-config.schema.json."
        }

        $sectionDefinitions = @()
        foreach ($definitionEntry in $CommonSchema.definitions.PSObject.Properties) {
            $definition = $definitionEntry.Value
            $docMetadata = Get-OptionalPropertyValue -Node $definition -PropertyName 'x-bgraster-doc'
            if ($null -eq $docMetadata) {
                continue
            }

            $sectionDefinitions += [pscustomobject]@{
                Name = $definitionEntry.Name
                Definition = $definition
                Doc = $docMetadata
                Order = [int](Get-OptionalPropertyValue -Node $docMetadata -PropertyName 'order')
            }
        }

        if ($sectionDefinitions.Count -eq 0) {
            throw "No per-definition x-bgraster-doc metadata found in docs/schemas/bgraster-common.schema.json."
        }

        $sectionDefinitions = $sectionDefinitions | Sort-Object Order, Name

        $lines = @()
        $isFirstSection = $true

        foreach ($sectionDefinition in $sectionDefinitions) {
            $section = $sectionDefinition.Doc
            $sectionNode = $sectionDefinition.Definition

            if (-not $isFirstSection) {
                $lines += ''
                $lines += '---'
                $lines += ''
            }

            $isFirstSection = $false
            $lines += ('## `{0}`' -f $section.heading)
            $lines += ''

            $sectionIntro = [string](Get-OptionalPropertyValue -Node $section -PropertyName 'intro')
            $sectionDescription = [string](Get-OptionalPropertyValue -Node $sectionNode -PropertyName 'description')
            $intro = if ([string]::IsNullOrWhiteSpace($sectionIntro)) { $sectionDescription } else { $sectionIntro }
            if (-not [string]::IsNullOrWhiteSpace($intro)) {
                $lines += $intro
                $lines += ''
            }

            $showDefaults = $true
            if ($section.PSObject.Properties.Name -contains 'showDefaults') {
                $showDefaults = [bool]$section.showDefaults
            }

            if ($showDefaults) {
                $lines += '| Key | Type | Default | Description |'
                $lines += '|---|---|---|---|'
            }
            else {
                $lines += '| Key | Type | Description |'
                $lines += '|---|---|---|'
            }

            foreach ($entry in $sectionNode.properties.PSObject.Properties) {
                $propertyName = [string]$entry.Name
                $propertySchema = $entry.Value
                $resolved = $propertySchema
                if ($propertySchema.PSObject.Properties.Name -contains '$ref') {
                    $resolved = Resolve-SchemaRefNode -Ref ([string]$propertySchema.'$ref') -ConfigSchema $ConfigSchema -CommonSchema $CommonSchema
                }

                $typeName = Escape-MarkdownCell (Get-SchemaTypeName -Node $propertySchema)
                $description = [string](Get-OptionalPropertyValue -Node $propertySchema -PropertyName 'description')
                if ([string]::IsNullOrWhiteSpace($description)) {
                    $description = [string](Get-OptionalPropertyValue -Node $resolved -PropertyName 'description')
                }

                $description = ('{0}{1}' -f $description, (Get-EnumDescriptionSuffix -PrimaryNode $propertySchema -ResolvedNode $resolved))

                $description = Escape-MarkdownCell $description

                if ($showDefaults) {
                    $defaultSource = if ($propertySchema.PSObject.Properties.Name -contains 'default') { $propertySchema.default } else { $resolved.default }
                    $defaultValue = Escape-MarkdownCell (Get-DefaultLiteral -Value $defaultSource)
                    $lines += ('| `{0}` | `{1}` | `{2}` | {3} |' -f $propertyName, $typeName, $defaultValue, $description)
                }
                else {
                    $lines += ('| `{0}` | `{1}` | {2} |' -f $propertyName, $typeName, $description)
                }
            }

            if ($section.PSObject.Properties.Name -contains 'notes' -and $null -ne $section.notes -and $section.notes.Count -gt 0) {
                $lines += ''
                foreach ($note in $section.notes) {
                    $lines += ('- {0}' -f $note)
                }
            }
        }

        if ($reference.PSObject.Properties.Name -contains 'appendix') {
            $appendix = $reference.appendix

            $lines += ''
            $lines += '---'
            $lines += ''
            $lines += '## Units'
            $lines += ''
            $lines += '| Suffix | Meaning |'
            $lines += '|---|---|'
            foreach ($row in $appendix.units) {
                $lines += ('| `{0}` | {1} |' -f $row.suffix, (Escape-MarkdownCell -Value ([string]$row.meaning)))
            }

            $lines += ''
            $lines += '## colors'
            $lines += ''
            $lines += '| Format | Example |'
            $lines += '|---|---|'
            foreach ($row in $appendix.colors) {
                $lines += ('| {0} | `{1}` |' -f (Escape-MarkdownCell -Value ([string]$row.format)), (Escape-MarkdownCell -Value ([string]$row.example)))
            }

            $lines += ''
            $lines += '## Substitution tokens'
            $lines += ''
            $lines += 'These tokens are expanded inside text, `background.image`, `logo.source`, and `render.output` values:'
            $lines += ''
            $lines += '| Token (machine scoped) | Token (output scoped) | Token (slice scoped) | Description |'
            $lines += '|---|---|---|---|'
            foreach ($row in $appendix.substitutionTokens) {
                $lines += ('| `{0}` | `{1}` | `{2}` | {3} |' -f $row.machineScoped, $row.outputScoped, $row.sliceScoped, (Escape-MarkdownCell -Value ([string]$row.description)))
            }
        }

        return $lines -join "`n"
    }

    Write-GeneratedMarkdownFile -FileName "cli-schema.md" -Content (ConvertTo-CliOptionsTable -Schema $schema -CommonSchema $commonSchema)
    Write-GeneratedMarkdownFile -FileName "toml-root-scalars.md" -Content (ConvertTo-TomlRootScalarsTable -Schema $schema)
    Write-GeneratedMarkdownFile -FileName "toml-schema-sections.md" -Content (ConvertTo-TomlSectionsMarkdown -ConfigSchema $schema -CommonSchema $commonSchema)
    Sync-BrandingAssets

    # Generate docs/index.md from README.md, rewriting repo-root-relative paths to docs/-relative paths.
    $readmePath = Join-Path $repoRoot "README.md"
    $indexContent = Get-Content -Raw -Path $readmePath
    # Strip the docs/ prefix from markdown links and image references so paths resolve correctly in MkDocs.
    $indexContent = $indexContent -replace '\]\(docs/', ']('
    # Rewrite README's repo-root logo path to the docs asset path synced by Sync-BrandingAssets.
    $indexContent = $indexContent -replace 'src="resources/gsp\.svg"', 'src="assets/images/logo.svg"'
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
        $renderTemplate = Join-Path $SampleOutputDirectory "$stem`_{index}"

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
