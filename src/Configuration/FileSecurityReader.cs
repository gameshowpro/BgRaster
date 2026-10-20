// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

using GameshowPro.BgRaster.Configuration.Interop;
using Microsoft.Win32.SafeHandles;

namespace GameshowPro.BgRaster.Configuration;

/// <summary>
/// Reads the owner, DACL and reparse-point flag of a file or directory into a <see cref="FileSecuritySnapshot"/>.
/// The object is opened without following reparse points, so the flag and the ACL describe the same object.
/// </summary>
internal static class FileSecurityReader
{
    private const int AclAceCountOffset = 4;
    private const int AceHeaderSize = 4;
    private const int SidHeaderSize = 8;

    /// <summary>
    /// Returns the security snapshot for <paramref name="path"/>, or null if anything about it could not be read
    /// or the DACL contains an ACE type the trust rule does not understand. Callers treat null as untrusted.
    /// </summary>
    internal static FileSecuritySnapshot? TryRead(string path)
    {
        using SafeFileHandle handle = NativeMethods.CreateFile(
            path,
            NativeMethods.READ_CONTROL | NativeMethods.FILE_READ_ATTRIBUTES,
            NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE | NativeMethods.FILE_SHARE_DELETE,
            0,
            NativeMethods.OPEN_EXISTING,
            NativeMethods.FILE_FLAG_BACKUP_SEMANTICS | NativeMethods.FILE_FLAG_OPEN_REPARSE_POINT,
            0);

        if (handle.IsInvalid)
        {
            return null;
        }

        if (!NativeMethods.GetFileInformationByHandleEx(
                handle,
                NativeMethods.FileAttributeTagInfo,
                out FILE_ATTRIBUTE_TAG_INFO tagInfo,
                (uint)Marshal.SizeOf<FILE_ATTRIBUTE_TAG_INFO>()))
        {
            return null;
        }

        bool isReparsePoint = (tagInfo.FileAttributes & NativeMethods.FILE_ATTRIBUTE_REPARSE_POINT) != 0;

        uint result = NativeMethods.GetSecurityInfo(
            handle,
            NativeMethods.SE_FILE_OBJECT,
            NativeMethods.OWNER_SECURITY_INFORMATION | NativeMethods.DACL_SECURITY_INFORMATION,
            out nint ownerSid,
            0,
            out nint dacl,
            0,
            out nint securityDescriptor);

        if (result != NativeMethods.ERROR_SUCCESS)
        {
            return null;
        }

        try
        {
            string? owner = null;
            if (ownerSid != 0)
            {
                owner = ConvertSidToString(ownerSid);
                if (owner is null)
                {
                    return null;
                }
            }

            // A null DACL pointer means the descriptor has a null DACL (or none at all): everyone has full access.
            if (dacl == 0)
            {
                return new FileSecuritySnapshot(isReparsePoint, owner, Dacl: null);
            }

            List<SecurityAce>? aces = ReadAces(dacl);
            return aces is null ? null : new FileSecuritySnapshot(isReparsePoint, owner, aces);
        }
        finally
        {
            _ = NativeMethods.LocalFree(securityDescriptor);
        }
    }

    private static List<SecurityAce>? ReadAces(nint dacl)
    {
        int aceCount = (ushort)Marshal.ReadInt16(dacl, AclAceCountOffset);
        List<SecurityAce> aces = new(aceCount);

        for (int i = 0; i < aceCount; i++)
        {
            if (!NativeMethods.GetAce(dacl, (uint)i, out nint ace) || ace == 0)
            {
                return null;
            }

            byte aceType = Marshal.ReadByte(ace, 0);
            byte aceFlags = Marshal.ReadByte(ace, 1);
            int aceSize = (ushort)Marshal.ReadInt16(ace, 2);

            bool isAllow;
            bool isObjectAce;
            switch (aceType)
            {
                case NativeMethods.ACCESS_ALLOWED_ACE_TYPE:
                case NativeMethods.ACCESS_ALLOWED_CALLBACK_ACE_TYPE:
                    isAllow = true;
                    isObjectAce = false;
                    break;
                case NativeMethods.ACCESS_ALLOWED_OBJECT_ACE_TYPE:
                case NativeMethods.ACCESS_ALLOWED_CALLBACK_OBJECT_ACE_TYPE:
                    isAllow = true;
                    isObjectAce = true;
                    break;
                case NativeMethods.ACCESS_DENIED_ACE_TYPE:
                case NativeMethods.ACCESS_DENIED_CALLBACK_ACE_TYPE:
                    isAllow = false;
                    isObjectAce = false;
                    break;
                case NativeMethods.ACCESS_DENIED_OBJECT_ACE_TYPE:
                case NativeMethods.ACCESS_DENIED_CALLBACK_OBJECT_ACE_TYPE:
                    isAllow = false;
                    isObjectAce = true;
                    break;
                default:
                    // Unknown or unexpected ACE type (for example a compound ACE): fail closed rather than guess.
                    return null;
            }

            if (aceSize < AceHeaderSize + 4)
            {
                return null;
            }

            uint accessMask = (uint)Marshal.ReadInt32(ace, AceHeaderSize);
            int sidOffset = AceHeaderSize + 4;

            if (isObjectAce)
            {
                if (aceSize < sidOffset + 4)
                {
                    return null;
                }

                uint objectFlags = (uint)Marshal.ReadInt32(ace, sidOffset);
                sidOffset += 4;
                if ((objectFlags & NativeMethods.ACE_OBJECT_TYPE_PRESENT) != 0)
                {
                    sidOffset += 16;
                }

                if ((objectFlags & NativeMethods.ACE_INHERITED_OBJECT_TYPE_PRESENT) != 0)
                {
                    sidOffset += 16;
                }
            }

            if (aceSize < sidOffset + SidHeaderSize)
            {
                return null;
            }

            int subAuthorityCount = Marshal.ReadByte(ace, sidOffset + 1);
            if (aceSize < sidOffset + SidHeaderSize + (4 * subAuthorityCount))
            {
                return null;
            }

            string? sid = ConvertSidToString(ace + sidOffset);
            if (sid is null)
            {
                return null;
            }

            aces.Add(new SecurityAce(
                sid,
                accessMask,
                isAllow,
                IsInheritOnly: (aceFlags & NativeMethods.INHERIT_ONLY_ACE) != 0));
        }

        return aces;
    }

    private static string? ConvertSidToString(nint sid)
    {
        if (!NativeMethods.ConvertSidToStringSid(sid, out nint stringSid) || stringSid == 0)
        {
            return null;
        }

        try
        {
            return Marshal.PtrToStringUni(stringSid);
        }
        finally
        {
            _ = NativeMethods.LocalFree(stringSid);
        }
    }
}
