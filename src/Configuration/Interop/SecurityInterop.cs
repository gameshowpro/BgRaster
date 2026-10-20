// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

using Microsoft.Win32.SafeHandles;

namespace GameshowPro.BgRaster.Configuration.Interop;

internal static partial class NativeMethods
{
    internal const uint READ_CONTROL = 0x0002_0000;
    internal const uint FILE_READ_ATTRIBUTES = 0x0000_0080;

    internal const uint FILE_SHARE_READ = 0x0000_0001;
    internal const uint FILE_SHARE_WRITE = 0x0000_0002;
    internal const uint FILE_SHARE_DELETE = 0x0000_0004;

    internal const uint OPEN_EXISTING = 3;

    internal const uint FILE_FLAG_BACKUP_SEMANTICS = 0x0200_0000;
    internal const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x0020_0000;

    internal const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x0000_0400;

    internal const int FileAttributeTagInfo = 9;

    internal const int SE_FILE_OBJECT = 1;

    internal const uint OWNER_SECURITY_INFORMATION = 0x0000_0001;
    internal const uint DACL_SECURITY_INFORMATION = 0x0000_0004;

    internal const uint ERROR_SUCCESS = 0;

    internal const byte ACCESS_ALLOWED_ACE_TYPE = 0x00;
    internal const byte ACCESS_DENIED_ACE_TYPE = 0x01;
    internal const byte ACCESS_ALLOWED_OBJECT_ACE_TYPE = 0x05;
    internal const byte ACCESS_DENIED_OBJECT_ACE_TYPE = 0x06;
    internal const byte ACCESS_ALLOWED_CALLBACK_ACE_TYPE = 0x09;
    internal const byte ACCESS_DENIED_CALLBACK_ACE_TYPE = 0x0A;
    internal const byte ACCESS_ALLOWED_CALLBACK_OBJECT_ACE_TYPE = 0x0B;
    internal const byte ACCESS_DENIED_CALLBACK_OBJECT_ACE_TYPE = 0x0C;

    internal const byte INHERIT_ONLY_ACE = 0x08;

    internal const uint ACE_OBJECT_TYPE_PRESENT = 0x1;
    internal const uint ACE_INHERITED_OBJECT_TYPE_PRESENT = 0x2;

    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    internal static partial SafeFileHandle CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        nint lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        nint hTemplateFile);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetFileInformationByHandleEx(
        SafeFileHandle hFile,
        int fileInformationClass,
        out FILE_ATTRIBUTE_TAG_INFO lpFileInformation,
        uint dwBufferSize);

    [LibraryImport("advapi32.dll")]
    internal static partial uint GetSecurityInfo(
        SafeFileHandle handle,
        int objectType,
        uint securityInfo,
        out nint ppsidOwner,
        nint ppsidGroup,
        out nint ppDacl,
        nint ppSacl,
        out nint ppSecurityDescriptor);

    [LibraryImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetAce(nint pAcl, uint dwAceIndex, out nint pAce);

    [LibraryImport("advapi32.dll", EntryPoint = "ConvertSidToStringSidW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ConvertSidToStringSid(nint pSid, out nint stringSid);

    [LibraryImport("kernel32.dll")]
    internal static partial nint LocalFree(nint hMem);
}

[StructLayout(LayoutKind.Sequential)]
internal struct FILE_ATTRIBUTE_TAG_INFO
{
    public uint FileAttributes;
    public uint ReparseTag;
}
