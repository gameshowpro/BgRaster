// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.FileLifecycle.Interop;

internal static partial class NativeMethods
{
    internal const uint FO_DELETE = 0x0003;
    internal const ushort FOF_ALLOWUNDO = 0x0040;
    internal const ushort FOF_NOCONFIRMATION = 0x0010;
    internal const ushort FOF_NOERRORUI = 0x0400;
    internal const ushort FOF_SILENT = 0x0004;

    [LibraryImport("shell32.dll", EntryPoint = "SHFileOperationW")]
    [return: MarshalAs(UnmanagedType.I4)]
    internal static partial int SHFileOperation(ref SHFILEOPSTRUCTW lpFileOp);
}

[StructLayout(LayoutKind.Sequential)]
internal struct SHFILEOPSTRUCTW
{
    public IntPtr hwnd;
    public uint wFunc;
    public IntPtr pFrom;
    public IntPtr pTo;
    public ushort fFlags;
    public int fAnyOperationsAborted;
    public IntPtr hNameMappings;
    public IntPtr lpszProgressTitle;
}