// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Configuration;

/// <summary>One entry of a discretionary access control list, reduced to what the trust rule needs.</summary>
/// <param name="Sid">The trustee's SID in string form, for example <c>S-1-5-32-545</c>.</param>
/// <param name="AccessMask">The raw access mask, which can include generic rights.</param>
/// <param name="IsAllow">True for an access-allowed ACE; false for an access-denied ACE.</param>
/// <param name="IsInheritOnly">True when the ACE only propagates to children and does not apply to the object itself.</param>
internal readonly record struct SecurityAce(string Sid, uint AccessMask, bool IsAllow, bool IsInheritOnly);

/// <summary>The security facts about one file system object that the trust rule evaluates.</summary>
/// <param name="IsReparsePoint">True when the object is a junction, symbolic link or other reparse point.</param>
/// <param name="OwnerSid">The owner's SID in string form, or null when there is no owner.</param>
/// <param name="Dacl">The DACL entries in order, or null when the object has a null DACL (everyone has full access).</param>
internal sealed record FileSecuritySnapshot(bool IsReparsePoint, string? OwnerSid, IReadOnlyList<SecurityAce>? Dacl);

/// <summary>Access mask bits used by the trust rule. Values are from the Windows SDK (winnt.h).</summary>
internal static class FileAccessMask
{
    internal const uint WriteData = 0x0000_0002;               // FILE_WRITE_DATA / FILE_ADD_FILE
    internal const uint AppendData = 0x0000_0004;              // FILE_APPEND_DATA / FILE_ADD_SUBDIRECTORY
    internal const uint WriteExtendedAttributes = 0x0000_0010; // FILE_WRITE_EA
    internal const uint DeleteChild = 0x0000_0040;             // FILE_DELETE_CHILD (DeleteSubdirectoriesAndFiles)
    internal const uint WriteAttributes = 0x0000_0100;         // FILE_WRITE_ATTRIBUTES
    internal const uint Delete = 0x0001_0000;                  // DELETE
    internal const uint ReadControl = 0x0002_0000;             // READ_CONTROL
    internal const uint WriteDac = 0x0004_0000;                // WRITE_DAC (ChangePermissions)
    internal const uint WriteOwner = 0x0008_0000;              // WRITE_OWNER (TakeOwnership)
    internal const uint Synchronize = 0x0010_0000;             // SYNCHRONIZE
    internal const uint GenericAll = 0x1000_0000;              // GENERIC_ALL
    internal const uint GenericWrite = 0x4000_0000;            // GENERIC_WRITE

    internal const uint FullControl = 0x001F_01FF;             // FILE_ALL_ACCESS
    internal const uint ReadAndExecute = 0x0012_00A9;          // FILE_GENERIC_READ | FILE_GENERIC_EXECUTE
}

/// <summary>
/// Decides whether the machine-wide config is controlled by administrators.
/// Any standard user can create folders under %ProgramData%, so without this check whoever creates
/// %ProgramData%\BgRaster first could control every other user's BgRaster runs.
/// </summary>
internal static class ConfigLocationTrust
{
    internal const string AdministratorsSid = "S-1-5-32-544";
    internal const string LocalSystemSid = "S-1-5-18";
    internal const string TrustedInstallerSid = "S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
    internal const string OwnerRightsSid = "S-1-3-4";

    // Creating files in the directory (WriteData/AppendData) is deliberately allowed: any file a standard user
    // creates there is owned by them and fails the file rule, and admin-created ProgramData folders inherit it.
    internal const uint DirectoryForbiddenRights =
        FileAccessMask.Delete
        | FileAccessMask.DeleteChild
        | FileAccessMask.WriteDac
        | FileAccessMask.WriteOwner;

    internal const uint FileForbiddenRights =
        FileAccessMask.WriteData
        | FileAccessMask.AppendData
        | FileAccessMask.WriteExtendedAttributes
        | FileAccessMask.Delete
        | FileAccessMask.WriteDac
        | FileAccessMask.WriteOwner;

    // Generic rights in an ACE count as granting every write-class right.
    private const uint GenericWriteClassRights = FileAccessMask.GenericAll | FileAccessMask.GenericWrite;

    /// <summary>
    /// Reads the security of <paramref name="configFilePath"/> and its parent directory from disk and returns
    /// true only if both pass the trust rule. Any failure to read either object counts as not trusted.
    /// </summary>
    internal static bool IsAdministratorControlled(string configFilePath)
    {
        try
        {
            string? directory = Path.GetDirectoryName(Path.GetFullPath(configFilePath));
            if (string.IsNullOrEmpty(directory))
            {
                return false;
            }

            return IsDirectoryAdministratorControlled(FileSecurityReader.TryRead(directory))
                && IsFileAdministratorControlled(FileSecurityReader.TryRead(configFilePath));
        }
        catch (Exception)
        {
            // Fail closed: an unexpected error must never make an untrusted config look trusted.
            return false;
        }
    }

    /// <summary>Applies the directory rule. A null snapshot (security could not be read) fails.</summary>
    internal static bool IsDirectoryAdministratorControlled(FileSecuritySnapshot? snapshot) =>
        Evaluate(snapshot, DirectoryForbiddenRights);

    /// <summary>Applies the file rule. A null snapshot (security could not be read) fails.</summary>
    internal static bool IsFileAdministratorControlled(FileSecuritySnapshot? snapshot) =>
        Evaluate(snapshot, FileForbiddenRights);

    internal static bool IsAdministrativeSid(string sid) =>
        string.Equals(sid, AdministratorsSid, StringComparison.OrdinalIgnoreCase)
        || string.Equals(sid, LocalSystemSid, StringComparison.OrdinalIgnoreCase)
        || string.Equals(sid, TrustedInstallerSid, StringComparison.OrdinalIgnoreCase);

    private static bool Evaluate(FileSecuritySnapshot? snapshot, uint forbiddenRights)
    {
        if (snapshot is null
            || snapshot.IsReparsePoint
            || snapshot.OwnerSid is null
            || !IsAdministrativeSid(snapshot.OwnerSid)
            || snapshot.Dacl is null)
        {
            return false;
        }

        foreach (SecurityAce ace in snapshot.Dacl)
        {
            // Deny ACEs can only remove rights, and inherit-only ACEs do not apply to this object.
            if (!ace.IsAllow || ace.IsInheritOnly)
            {
                continue;
            }

            // OWNER RIGHTS applies to the owner, which is already required to be administrative.
            if (IsAdministrativeSid(ace.Sid) || string.Equals(ace.Sid, OwnerRightsSid, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if ((ace.AccessMask & (forbiddenRights | GenericWriteClassRights)) != 0)
            {
                return false;
            }
        }

        return true;
    }
}
