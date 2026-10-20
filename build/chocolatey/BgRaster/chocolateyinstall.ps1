# BgRaster Chocolatey install script
# BgRaster is a self-contained AOT-compiled executable. No runtime dependencies needed.

$ErrorActionPreference = 'Stop'

$packageName = $env:ChocolateyPackageName
$toolsDir = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)"
$binariesDir = Join-Path (Split-Path -Parent $toolsDir) 'binaries'

# BgRaster only trusts %ProgramData%\BgRaster\config.toml if administrators control it.
# This must match the rule in the app (ConfigLocationTrust). SIDs are compared, never names.
$script:AdminSids = @(
    'S-1-5-32-544',  # BUILTIN\Administrators
    'S-1-5-18',      # NT AUTHORITY\SYSTEM
    'S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464',  # NT SERVICE\TrustedInstaller
    'S-1-3-4'        # OWNER RIGHTS (ignored: the owner must be administrative anyway)
)
$script:AdminOwnerSids = $script:AdminSids | Select-Object -First 3

# Raw access mask bits, because GENERIC_* bits fall outside the FileSystemRights enum.
$script:GenericWriteBits = 0x10000000 -bor 0x40000000  # GENERIC_ALL, GENERIC_WRITE
$script:DirectoryDangerBits = 0x10000 -bor 0x40 -bor 0x40000 -bor 0x80000  # Delete, DeleteSubdirectoriesAndFiles, ChangePermissions, TakeOwnership
$script:FileDangerBits = 0x2 -bor 0x4 -bor 0x10 -bor 0x10000 -bor 0x40000 -bor 0x80000  # WriteData, AppendData, WriteExtendedAttributes, Delete, ChangePermissions, TakeOwnership

# Returns $null if administrators control the path, otherwise a short reason. Any failure means not controlled.
function Get-AdminControlProblem {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][ValidateSet('Directory', 'File')][string]$Kind
    )
    try {
        $item = Get-Item -LiteralPath $Path -Force
        if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) { return 'is a reparse point' }

        $descriptor = (Get-Acl -LiteralPath $Path).GetSecurityDescriptorBinaryForm()
        $raw = New-Object System.Security.AccessControl.RawSecurityDescriptor($descriptor, 0)
        if ($null -eq $raw.Owner -or $script:AdminOwnerSids -notcontains $raw.Owner.Value) { return 'is not owned by an administrative account' }
        if ($null -eq $raw.DiscretionaryAcl) { return 'has no access control list' }

        $dangerBits = if ($Kind -eq 'Directory') { $script:DirectoryDangerBits } else { $script:FileDangerBits }
        foreach ($ace in $raw.DiscretionaryAcl) {
            # Only allow ACEs matter, and inherit-only ACEs do not apply to the object itself.
            if ($ace -isnot [System.Security.AccessControl.QualifiedAce]) { return 'has an access control entry that cannot be checked' }
            if ($ace.AceQualifier -ne [System.Security.AccessControl.AceQualifier]::AccessAllowed) { continue }
            if ([int]$ace.AceFlags -band [int][System.Security.AccessControl.AceFlags]::InheritOnly) { continue }
            if ($script:AdminSids -contains $ace.SecurityIdentifier.Value) { continue }
            if ($ace.AccessMask -band ($dangerBits -bor $script:GenericWriteBits)) {
                return "grants write access to $($ace.SecurityIdentifier.Value)"
            }
        }
        return $null
    }
    catch {
        return "could not be checked ($($_.Exception.Message))"
    }
}

# Creates the folder writable only by administrators (owner Administrators, no inheritance).
function New-AdminControlledDirectory {
    param([Parameter(Mandatory)][string]$Path)
    $administrators = New-Object System.Security.Principal.SecurityIdentifier('S-1-5-32-544')
    $system = New-Object System.Security.Principal.SecurityIdentifier('S-1-5-18')
    $users = New-Object System.Security.Principal.SecurityIdentifier('S-1-5-32-545')
    $inherit = [System.Security.AccessControl.InheritanceFlags]'ContainerInherit, ObjectInherit'
    $none = [System.Security.AccessControl.PropagationFlags]::None
    $allow = [System.Security.AccessControl.AccessControlType]::Allow

    $null = New-Item -ItemType Directory -Path $Path
    $acl = New-Object System.Security.AccessControl.DirectorySecurity
    $acl.SetOwner($administrators)
    $acl.SetAccessRuleProtection($true, $false)
    foreach ($grant in @(@($system, 'FullControl'), @($administrators, 'FullControl'), @($users, 'ReadAndExecute'))) {
        $acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($grant[0], $grant[1], $inherit, $none, $allow)))
    }
    Set-Acl -LiteralPath $Path -AclObject $acl
}

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

# Prepare the machine-wide config folder. A problem here must never fail the package install.
try {
    $configDir = Join-Path $env:ProgramData 'BgRaster'
    if (-not (Test-Path -LiteralPath $configDir)) {
        New-AdminControlledDirectory -Path $configDir
        Write-Host "Created: $configDir (only administrators can change it)"
    }
    else {
        $problemPath = $configDir
        $problem = Get-AdminControlProblem -Path $configDir -Kind Directory
        $configFile = Join-Path $configDir 'config.toml'
        if (-not $problem -and (Test-Path -LiteralPath $configFile)) {
            $problemPath = $configFile
            $problem = Get-AdminControlProblem -Path $configFile -Kind File
        }
        if ($problem) {
            Write-Warning "'$problemPath' $problem, so BgRaster will ignore the machine-wide config until an administrator fixes or recreates it. Nothing was changed."
        }
    }
}
catch {
    Write-Warning "Could not prepare the machine-wide config folder: $($_.Exception.Message)"
}

Write-Host "BgRaster installed successfully. Run 'BgRaster --help' to get started."

Write-Host ""
Write-Host "BgRaster participates in the Open Source Maintenance Fee. Organizations that"
Write-Host "use it as part of revenue-generating activities pay a small monthly fee that"
Write-Host "funds maintenance. Who pays and how: https://bgraster.gameshow.pro/maintenance-fee/"
