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
        $sourceSvg = Join-Path $repoRoot "resources/BgRaster.svg"
        $sourcePng = Join-Path $repoRoot "resources/BgRaster.png"
        $sourceGsp = Join-Path $repoRoot "resources/gsp.svg"

        if (-not (Test-Path $sourceSvg)) {
            throw "Branding source file not found: $sourceSvg"
        }

        $docsImageDirectory = Join-Path $repoRoot "docs/assets/images"
        New-Item -ItemType Directory -Path $docsImageDirectory -Force | Out-Null

        Copy-Item -Path $sourceSvg -Destination (Join-Path $docsImageDirectory "favicon.svg") -Force
        if (-not (Test-Path $sourcePng)) {
            throw "Branding source file not found: $sourcePng"
        }
        Copy-Item -Path $sourcePng -Destination (Join-Path $docsImageDirectory "favicon.png") -Force
        Copy-Item -Path $sourceGsp -Destination (Join-Path $docsImageDirectory "gsp.svg") -Force
    }

        function Format-DefaultResolution {
                    param([string]$Value)
                    # Wrap config paths
                    $v = $Value -replace '\b(config\.toml)\b', '`$1`'
                    # Wrap TOML property paths: [section].key-name, [section].key.sub
                    $v = $v -replace '(\[[^\]]+\]\.[a-z_-]+(?:\.[a-z_-]+)*)', '`$1`'
                    # Wrap booleans
                    $v = $v -replace '\b(true|false)\b', '`$1`'
                    # Wrap JSON-like array literals (greedy but stops at first ])
                    $v = $v -replace '(\["[^]]*"\])', '`$1`'
                    # Wrap path patterns like %TEMP%/... 
                                        $v = $v -replace '(%\w+%[^\s,;.]*)', '`$1`'
                                        return $v
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

        $lines = @()
                $groups = $options | Where-Object { $null -ne $_.PSObject.Properties['category'] } | Group-Object { [string]$_.category }
                $categoryOrder = @('Frequent', 'Advanced', 'Appearance')

                foreach ($cat in $categoryOrder) {
                    $group = $groups | Where-Object { $_.Name -eq $cat } | Select-Object -First 1
                    if ($null -eq $group) { continue }

                    $lines += ''
                    $lines += ('### {0} options' -f $cat)
                    $lines += ''
                    $lines += '| Option | Type | TOML equivalent | Description | Default resolution |'
                    $lines += '|---|---|---|---|---|'

                    foreach ($option in $group.Group) {
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
                                        if (-not $description.EndsWith('.', [StringComparison]::Ordinal)) {
                                            $description += '.'
                                        }
                                        $description = ('{0}{1}' -f $description, $enumSuffix)
                                    }
                                }

                                $defaultResolution = if ([string]::IsNullOrWhiteSpace([string]$option.defaultResolution)) { '-' } else { Format-DefaultResolution ([string]$option.defaultResolution) }

                                                                $lines += ('| `{0}` | `{1}` | `{2}` | {3} | {4} |' -f $optionSyntax, $typeName, $tomlEquivalent, $description, $defaultResolution)
                            }
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
            if ($null -ne $itemsNode) {
                return @(Get-EnumValues -Node $itemsNode)
            }
        }

        if ($Node.PSObject.Properties.Name -contains 'oneOf') {
            $values = @()
            foreach ($item in $Node.oneOf) {
                $values += @(Get-EnumValues -Node $item)
            }
            return $values
        }

        if ($Node.PSObject.Properties.Name -contains 'anyOf') {
            $values = @()
            foreach ($item in $Node.anyOf) {
                $values += @(Get-EnumValues -Node $item)
            }
            return $values
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

        return (' Allowed enum values: {0}.' -f ($formattedValues -join ', '))
    }

    function Get-SchemaTypeName {
        param([object]$Node)

        if ($null -eq $Node) { return '-' }

        if ($Node.PSObject.Properties.Name -contains 'oneOf') {
            $types = @()
            foreach ($item in $Node.oneOf) {
                if ($item.PSObject.Properties.Name -contains 'type') {
                    $typeName = [string]$item.type
                    if ($item.PSObject.Properties.Name -contains 'enum') {
                        $typeName = 'enum'
                    }
                    $types += $typeName
                }
                elseif ($item.PSObject.Properties.Name -contains '$ref') {
                    $types += 'object'
                }
            }

            if ($types.Count -eq 0) { return 'oneOf' }
            return (($types | Select-Object -Unique) -join '` or `')
        }

        if ($Node.PSObject.Properties.Name -contains 'anyOf') {
            $types = @()
            foreach ($item in $Node.anyOf) {
                if ($item.PSObject.Properties.Name -contains 'type') {
                    if ($item.type -is [array]) {
                        foreach ($t in $item.type) { 
                            $typeName = [string]$t
                            if ($item.PSObject.Properties.Name -contains 'enum') {
                                $typeName = 'enum'
                            }
                            $types += $typeName 
                        }
                    } else {
                        $typeName = [string]$item.type
                        if ($item.PSObject.Properties.Name -contains 'enum') {
                            $typeName = 'enum'
                        }
                        $types += $typeName
                    }
                }
                elseif ($item.PSObject.Properties.Name -contains '$ref') {
                    $types += 'object'
                }
            }

            if ($types.Count -eq 0) { return 'anyOf' }
            return (($types | Select-Object -Unique) -join '` or `')
        }

        if (($Node.PSObject.Properties.Name -contains 'type') -and $Node.type -eq 'array') {
            $itemType = if ($Node.PSObject.Properties.Name -contains 'items') {
                Get-SchemaTypeName -Node $Node.items
            }
            else {
                'object'
            }

            if ($itemType -match '` or `') {
                return "($itemType)[]"
            }
            return "${itemType}[]"
        }

        if ($Node.PSObject.Properties.Name -contains 'type') {
            if ($Node.PSObject.Properties.Name -contains 'enum') {
                return 'enum'
            }
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
            if ($Value -is [System.Collections.IList] -and $Value.Count -eq 0) { return '[]' }
            return (ConvertTo-Json -InputObject $Value -Compress)
        }

        function Get-RefLinkHeading {
            param([string]$DefinitionName, [object]$CommonSchema)

            # If the definition itself has x-bgraster-doc, use its heading
            $def = $CommonSchema.definitions.PSObject.Properties[$DefinitionName]
            if ($def) {
                $docMeta = Get-OptionalPropertyValue -Node $def.Value -PropertyName 'x-bgraster-doc'
                if ($docMeta) {
                    return [string]$docMeta.heading
                }
            }

            # For override types, link to the corresponding main table
            if ($DefinitionName -match '^(.+)Override$') {
                $baseName = $Matches[1]
                $tableName = "${baseName}Table"
                $tableDef = $CommonSchema.definitions.PSObject.Properties[$tableName]
                if ($tableDef) {
                    $docMeta = Get-OptionalPropertyValue -Node $tableDef.Value -PropertyName 'x-bgraster-doc'
                    if ($docMeta) {
                        return [string]$docMeta.heading
                    }
                }
            }

            return $null
        }

        function Get-HeadingAnchor {
            param([string]$Heading)
            # Strip backticks, brackets, parens. Lowercase. Non-(alnum/./_/~/-) → hyphen.
                        $slug = $Heading -replace '[`\[\]()]', ''
                        $slug = $slug.ToLowerInvariant() -replace '[^a-z0-9._~-]+', '-'
                        $slug = $slug.Trim('-')
            return $slug
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

                            $typeName = Escape-MarkdownCell (Get-SchemaTypeName -Node $propertySchema)
                            $description = [string](Get-OptionalPropertyValue -Node $propertySchema -PropertyName 'description')
                            if ([string]::IsNullOrWhiteSpace($description)) {
                                $description = [string](Get-OptionalPropertyValue -Node $resolved -PropertyName 'description')
                            }

                            if ($propertySchema.PSObject.Properties.Name -contains '$ref') {
                                $resolved = Resolve-SchemaRefNode -Ref ([string]$propertySchema.'$ref') -ConfigSchema $ConfigSchema -CommonSchema $CommonSchema

                                # Fall back to resolved definition description if property has none
                                if ([string]::IsNullOrWhiteSpace($description)) {
                                    $description = [string](Get-OptionalPropertyValue -Node $resolved -PropertyName 'description')
                                }

                                # Append a link to the main definition section
                                $refDefName = [string]$propertySchema.'$ref'
                                $refDefName = $refDefName -replace '^#/definitions/', ''
                                $linkHeading = Get-RefLinkHeading -DefinitionName $refDefName -CommonSchema $CommonSchema
                                if ($linkHeading) {
                                    $anchor = Get-HeadingAnchor -Heading $linkHeading
                                    $linkSuffix = " See [$linkHeading](#$anchor)."
                                    $description = if ([string]::IsNullOrWhiteSpace($description)) { $linkSuffix.TrimStart() } else { "$description$linkSuffix" }
                                }
                            }

                $enumSuffix = Get-EnumDescriptionSuffix -PrimaryNode $propertySchema -ResolvedNode $resolved
                if (-not [string]::IsNullOrWhiteSpace($enumSuffix)) {
                    if (-not $description.EndsWith('.', [StringComparison]::Ordinal)) {
                        $description += '.'
                    }
                    $description = ('{0}{1}' -f $description, $enumSuffix)
                }

                $description = Escape-MarkdownCell $description

                if ($showDefaults) {
                    if ($propertySchema.PSObject.Properties.Name -contains 'default') {
                        $defaultSource = $propertySchema.default
                    } else {
                        $defaultSource = $resolved.default
                    }
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
                $m = if ([string]::IsNullOrEmpty($row.machineScoped)) { "" } else { ('`{0}`' -f $row.machineScoped) }
                $o = if ([string]::IsNullOrEmpty($row.outputScoped)) { "" } else { ('`{0}`' -f $row.outputScoped) }
                $s = if ([string]::IsNullOrEmpty($row.sliceScoped)) { "" } else { ('`{0}`' -f $row.sliceScoped) }
                $lines += ('| {0} | {1} | {2} | {3} |' -f $m, $o, $s, (Escape-MarkdownCell -Value ([string]$row.description)))
            }
        }

        return $lines -join "`n"
    }

    function ConvertTo-NetworkSectionsMarkdown {
        param(
            [object]$ConfigSchema,
            [object]$CommonSchema
        )

        $networkDef = $CommonSchema.definitions.PSObject.Properties['networkTable']
        if ($null -eq $networkDef) {
            throw "networkTable definition not found in common schema."
        }

        $definition = $networkDef.Value
        $docMetadata = Get-OptionalPropertyValue -Node $definition -PropertyName 'x-bgraster-doc'
        if ($null -eq $docMetadata) {
            throw "networkTable definition missing x-bgraster-doc metadata."
        }

        $sectionNode = $definition
        $section = $docMetadata

        $lines = @()

        $sectionIntro = [string](Get-OptionalPropertyValue -Node $section -PropertyName 'intro')
        $sectionDescription = [string](Get-OptionalPropertyValue -Node $sectionNode -PropertyName 'description')
        $intro = if ([string]::IsNullOrWhiteSpace($sectionIntro)) { $sectionDescription } else { $sectionIntro }
        if (-not [string]::IsNullOrWhiteSpace($intro)) {
            $lines += $intro
            $lines += ''
        }

        # Filters sub-table (first 8 properties)
        $lines += '### Filters'
        $lines += ''
        $lines += '| Key | Type | Default | Description |'
        $lines += '|---|---|---|---|'

        $filterKeys = @('require_adapter_types', 'exclude_adapter_types', 'require_up', 'require_family',
                        'require_mac_addresses', 'include_subnets', 'include_names', 'include_descriptions')

        foreach ($key in $filterKeys) {
            $entry = $sectionNode.properties.PSObject.Properties[$key]
            if ($null -eq $entry) { continue }
            $propertySchema = $entry.Value
            $resolved = $propertySchema
            if ($propertySchema.PSObject.Properties.Name -contains '$ref') {
                $resolved = Resolve-SchemaRefNode -Ref ([string]$propertySchema.'$ref') -ConfigSchema $ConfigSchema -CommonSchema $CommonSchema
            }
            $typeName = Escape-MarkdownCell (Get-SchemaTypeName -Node $propertySchema)
            $desc = [string](Get-OptionalPropertyValue -Node $propertySchema -PropertyName 'description')
            if ([string]::IsNullOrWhiteSpace($desc)) {
                $desc = [string](Get-OptionalPropertyValue -Node $resolved -PropertyName 'description')
            }
            $desc = Escape-MarkdownCell $desc
            if ($propertySchema.PSObject.Properties.Name -contains 'default') {
                $defaultSource = $propertySchema.default
            } else {
                $defaultSource = $resolved.default
            }
            $defaultValue = Escape-MarkdownCell (Get-DefaultLiteral -Value $defaultSource)
            $lines += ('| `{0}` | `{1}` | `{2}` | {3} |' -f $key, $typeName, $defaultValue, $desc)
        }

        # Settings sub-table (last 2 properties)
        $lines += ''
        $lines += '### Settings'
        $lines += ''
        $lines += '| Key | Type | Default | Description |'
        $lines += '|---|---|---|---|'

        $settingKeys = @('ip_address_format', 'adapter_format')

        foreach ($key in $settingKeys) {
            $entry = $sectionNode.properties.PSObject.Properties[$key]
            if ($null -eq $entry) { continue }
            $propertySchema = $entry.Value
            $resolved = $propertySchema
            if ($propertySchema.PSObject.Properties.Name -contains '$ref') {
                $resolved = Resolve-SchemaRefNode -Ref ([string]$propertySchema.'$ref') -ConfigSchema $ConfigSchema -CommonSchema $CommonSchema
            }
            $typeName = Escape-MarkdownCell (Get-SchemaTypeName -Node $propertySchema)
            $desc = [string](Get-OptionalPropertyValue -Node $propertySchema -PropertyName 'description')
            if ([string]::IsNullOrWhiteSpace($desc)) {
                $desc = [string](Get-OptionalPropertyValue -Node $resolved -PropertyName 'description')
            }
            $desc = Escape-MarkdownCell $desc
            if ($propertySchema.PSObject.Properties.Name -contains 'default') {
                $defaultSource = $propertySchema.default
            } else {
                $defaultSource = $resolved.default
            }
            $defaultValue = Escape-MarkdownCell (Get-DefaultLiteral -Value $defaultSource)
            $lines += ('| `{0}` | `{1}` | `{2}` | {3} |' -f $key, $typeName, $defaultValue, $desc)
        }


        return $lines -join "`n"
    }

    Write-GeneratedMarkdownFile -FileName "cli-schema.md" -Content (ConvertTo-CliOptionsTable -Schema $schema -CommonSchema $commonSchema)
    Write-GeneratedMarkdownFile -FileName "toml-schema-sections.md" -Content (ConvertTo-TomlSectionsMarkdown -ConfigSchema $schema -CommonSchema $commonSchema)
    Write-GeneratedMarkdownFile -FileName "network-sections.md" -Content (ConvertTo-NetworkSectionsMarkdown -ConfigSchema $schema -CommonSchema $commonSchema)
    Sync-BrandingAssets

    # Generate docs/index.md from README.md, rewriting repo-root-relative paths to docs/-relative paths.
    $readmePath = Join-Path $repoRoot "README.md"
    $indexContent = Get-Content -Raw -Path $readmePath
    # Strip the docs/ prefix from markdown links and image references so paths resolve correctly in MkDocs.
    $indexContent = $indexContent -replace '\]\(docs/', ']('
    $docsIndexPath = Join-Path $repoRoot "docs/index.md"
    # Rewrite README resource paths to the images synced by Sync-BrandingAssets.
    $indexContent = $indexContent -replace 'src="resources/BgRaster\.svg"', 'src="assets/images/favicon.svg"'
    $indexContent = $indexContent -replace 'src="resources/gsp\.svg"', 'src="assets/images/gsp.svg"'
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
