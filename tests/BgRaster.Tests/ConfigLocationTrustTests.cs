// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using GameshowPro.BgRaster;

namespace GameshowPro.BgRaster.Tests;

public class ConfigLocationTrustTests
{
    private const string Administrators = ConfigLocationTrust.AdministratorsSid;
    private const string LocalSystem = ConfigLocationTrust.LocalSystemSid;
    private const string TrustedInstaller = ConfigLocationTrust.TrustedInstallerSid;
    private const string OwnerRights = ConfigLocationTrust.OwnerRightsSid;
    private const string CreatorOwner = "S-1-3-0";
    private const string Users = "S-1-5-32-545";
    private const string StandardUser = "S-1-5-21-1111111111-2222222222-3333333333-1001";

    // Users: create files / create folders / write attributes, as an admin-created folder under ProgramData inherits it.
    private const uint UsersCreateFilesAndFolders =
        FileAccessMask.WriteData
        | FileAccessMask.AppendData
        | FileAccessMask.WriteExtendedAttributes
        | FileAccessMask.WriteAttributes
        | FileAccessMask.Synchronize;

    [Fact]
    public void Directory_AdminOwnedWithDefaultProgramDataInheritance_Passes()
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataDirectory();

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeTrue();
    }

    [Theory]
    [InlineData(Administrators)]
    [InlineData(LocalSystem)]
    [InlineData(TrustedInstaller)]
    public void Directory_OwnedByEachAdministrativeSid_Passes(string ownerSid)
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataDirectory() with { OwnerSid = ownerSid };

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeTrue();
    }

    [Fact]
    public void Directory_NonAdminOwner_Fails()
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataDirectory() with { OwnerSid = StandardUser };

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Directory_NoOwner_Fails()
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataDirectory() with { OwnerSid = null };

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Theory]
    [InlineData(FileAccessMask.Delete)]
    [InlineData(FileAccessMask.DeleteChild)]
    [InlineData(FileAccessMask.WriteDac)]
    [InlineData(FileAccessMask.WriteOwner)]
    [InlineData(FileAccessMask.GenericAll)]
    [InlineData(FileAccessMask.GenericWrite)]
    public void Directory_UsersGrantedForbiddenRight_Fails(uint right)
    {
        FileSecuritySnapshot snapshot = WithAce(DefaultProgramDataDirectory(), Allow(Users, right));

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Theory]
    [InlineData(FileAccessMask.WriteData)]
    [InlineData(FileAccessMask.AppendData)]
    [InlineData(FileAccessMask.WriteExtendedAttributes)]
    [InlineData(FileAccessMask.WriteAttributes)]
    public void Directory_UsersGrantedCreateOrAttributeRight_Passes(uint right)
    {
        FileSecuritySnapshot snapshot = WithAce(DefaultProgramDataDirectory(), Allow(Users, right));

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeTrue();
    }

    [Fact]
    public void Directory_InheritOnlyCreatorOwnerAce_IsIgnored()
    {
        FileSecuritySnapshot inheritOnly = AdminOnly(Allow(CreatorOwner, FileAccessMask.GenericAll, inheritOnly: true));
        FileSecuritySnapshot effective = AdminOnly(Allow(CreatorOwner, FileAccessMask.GenericAll));

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(inheritOnly).Should().BeTrue();
        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(effective).Should().BeFalse();
    }

    [Fact]
    public void Directory_DenyAce_IsIgnored()
    {
        FileSecuritySnapshot snapshot = AdminOnly(Deny(Users, FileAccessMask.FullControl), Deny(StandardUser, FileAccessMask.GenericAll));

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeTrue();
    }

    [Fact]
    public void Directory_OwnerRightsAce_IsIgnored()
    {
        FileSecuritySnapshot snapshot = AdminOnly(Allow(OwnerRights, FileAccessMask.FullControl));

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeTrue();
    }

    [Fact]
    public void Directory_ReparsePoint_Fails()
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataDirectory() with { IsReparsePoint = true };

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Directory_NullDacl_Fails()
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataDirectory() with { Dacl = null };

        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Fact]
    public void Directory_UnreadableSecurity_Fails()
    {
        _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(null).Should().BeFalse();
    }

    [Fact]
    public void File_AdminOwnedAndReadOnlyForUsers_Passes()
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataFile();

        _ = ConfigLocationTrust.IsFileAdministratorControlled(snapshot).Should().BeTrue();
    }

    [Fact]
    public void File_UsersWriteData_Fails()
    {
        FileSecuritySnapshot snapshot = WithAce(DefaultProgramDataFile(), Allow(Users, FileAccessMask.WriteData));

        _ = ConfigLocationTrust.IsFileAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Theory]
    [InlineData(FileAccessMask.WriteData)]
    [InlineData(FileAccessMask.AppendData)]
    [InlineData(FileAccessMask.WriteExtendedAttributes)]
    [InlineData(FileAccessMask.Delete)]
    [InlineData(FileAccessMask.WriteDac)]
    [InlineData(FileAccessMask.WriteOwner)]
    [InlineData(FileAccessMask.GenericAll)]
    [InlineData(FileAccessMask.GenericWrite)]
    public void File_StandardUserGrantedForbiddenRight_Fails(uint right)
    {
        FileSecuritySnapshot snapshot = WithAce(DefaultProgramDataFile(), Allow(StandardUser, right));

        _ = ConfigLocationTrust.IsFileAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Fact]
    public void File_NonAdminOwner_Fails()
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataFile() with { OwnerSid = StandardUser };

        _ = ConfigLocationTrust.IsFileAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Fact]
    public void File_ReparsePoint_Fails()
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataFile() with { IsReparsePoint = true };

        _ = ConfigLocationTrust.IsFileAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Fact]
    public void File_NullDacl_Fails()
    {
        FileSecuritySnapshot snapshot = DefaultProgramDataFile() with { Dacl = null };

        _ = ConfigLocationTrust.IsFileAdministratorControlled(snapshot).Should().BeFalse();
    }

    [Fact]
    public void File_UnreadableSecurity_Fails()
    {
        _ = ConfigLocationTrust.IsFileAdministratorControlled(null).Should().BeFalse();
    }

    [Fact]
    public void AdministrativeSids_AreComparedBySid_NotByName()
    {
        _ = ConfigLocationTrust.IsAdministrativeSid(Administrators).Should().BeTrue();
        _ = ConfigLocationTrust.IsAdministrativeSid(LocalSystem).Should().BeTrue();
        _ = ConfigLocationTrust.IsAdministrativeSid(TrustedInstaller).Should().BeTrue();
        _ = ConfigLocationTrust.IsAdministrativeSid(@"BUILTIN\Administrators").Should().BeFalse();
        _ = ConfigLocationTrust.IsAdministrativeSid(Users).Should().BeFalse();
        _ = ConfigLocationTrust.IsAdministrativeSid(OwnerRights).Should().BeFalse();
    }

    [Fact]
    public void IsAdministratorControlled_FreshTempDirectoryOwnedByTestUser_Fails()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            string configPath = Path.Combine(tempRoot, "config.toml");
            File.WriteAllText(configPath, "[render]\n");

            FileSecuritySnapshot? directorySnapshot = FileSecurityReader.TryRead(tempRoot);
            _ = directorySnapshot.Should().NotBeNull();
            _ = directorySnapshot!.IsReparsePoint.Should().BeFalse();
            _ = directorySnapshot.Dacl.Should().NotBeNull();

            if (IsElevated())
            {
                // Elevated processes create objects owned by Administrators, so make the failure independent
                // of ownership by granting a non-administrative SID a forbidden right.
                GrantUsersDelete(tempRoot);
            }
            else
            {
                _ = directorySnapshot.OwnerSid.Should().Be(CurrentUserSid());
            }

            _ = ConfigLocationTrust.IsAdministratorControlled(configPath).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void IsAdministratorControlled_SystemFile_Passes()
    {
        // Windows system files are owned by TrustedInstaller and read-only for everyone else, so this checks
        // that the reader produces snapshots the evaluator can pass, not only ones it rejects.
        string systemFile = Path.Combine(Environment.SystemDirectory, "kernel32.dll");

        _ = ConfigLocationTrust.IsAdministratorControlled(systemFile).Should().BeTrue();
    }

    [Fact]
    public void IsAdministratorControlled_MissingFile_Fails()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            _ = ConfigLocationTrust.IsAdministratorControlled(Path.Combine(tempRoot, "missing.toml")).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void TryRead_Junction_ReportsReparsePoint()
    {
        string tempRoot = CreateTempDirectory();
        try
        {
            string target = Path.Combine(tempRoot, "target");
            string junction = Path.Combine(tempRoot, "junction");
            _ = Directory.CreateDirectory(target);

            using Process? mklink = Process.Start(new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{junction}\" \"{target}\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            _ = mklink.Should().NotBeNull();
            mklink!.WaitForExit();
            _ = mklink.ExitCode.Should().Be(0);

            FileSecuritySnapshot? snapshot = FileSecurityReader.TryRead(junction);

            _ = snapshot.Should().NotBeNull();
            _ = snapshot!.IsReparsePoint.Should().BeTrue();
            _ = ConfigLocationTrust.IsDirectoryAdministratorControlled(snapshot).Should().BeFalse();
        }
        finally
        {
            string junction = Path.Combine(tempRoot, "junction");
            if (Directory.Exists(junction))
            {
                Directory.Delete(junction);
            }

            Directory.Delete(tempRoot, recursive: true);
        }
    }

    // The ACL an administrator gets when creating %ProgramData%\BgRaster with default inheritance.
    private static FileSecuritySnapshot DefaultProgramDataDirectory() =>
        new(
            IsReparsePoint: false,
            OwnerSid: Administrators,
            Dacl:
            [
                Allow(LocalSystem, FileAccessMask.FullControl),
                Allow(Administrators, FileAccessMask.FullControl),
                Allow(CreatorOwner, FileAccessMask.GenericAll, inheritOnly: true),
                Allow(Users, FileAccessMask.ReadAndExecute),
                Allow(Users, UsersCreateFilesAndFolders),
            ]);

    // The ACL an administrator gets when creating config.toml in that folder: the create-files ACE does not reach files.
    private static FileSecuritySnapshot DefaultProgramDataFile() =>
        new(
            IsReparsePoint: false,
            OwnerSid: Administrators,
            Dacl:
            [
                Allow(LocalSystem, FileAccessMask.FullControl),
                Allow(Administrators, FileAccessMask.FullControl),
                Allow(Users, FileAccessMask.ReadAndExecute),
            ]);

    private static FileSecuritySnapshot AdminOnly(params SecurityAce[] extraAces) =>
        new(
            IsReparsePoint: false,
            OwnerSid: Administrators,
            Dacl:
            [
                Allow(LocalSystem, FileAccessMask.FullControl),
                Allow(Administrators, FileAccessMask.FullControl),
                .. extraAces,
            ]);

    private static FileSecuritySnapshot WithAce(FileSecuritySnapshot snapshot, SecurityAce ace) =>
        snapshot with { Dacl = [.. snapshot.Dacl!, ace] };

    private static SecurityAce Allow(string sid, uint mask, bool inheritOnly = false) =>
        new(sid, mask, IsAllow: true, IsInheritOnly: inheritOnly);

    private static SecurityAce Deny(string sid, uint mask) =>
        new(sid, mask, IsAllow: false, IsInheritOnly: false);

    private static bool IsElevated() =>
        OperatingSystem.IsWindows()
        && new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    private static string? CurrentUserSid() =>
        OperatingSystem.IsWindows() ? WindowsIdentity.GetCurrent().User?.Value : null;

    private static void GrantUsersDelete(string directory)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        DirectoryInfo info = new(directory);
        DirectorySecurity security = info.GetAccessControl();
        security.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
            FileSystemRights.Delete,
            AccessControlType.Allow));
        info.SetAccessControl(security);
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"bgraster-tests-{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(path);
        return path;
    }
}
