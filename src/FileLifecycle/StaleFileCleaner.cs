// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.FileLifecycle;

using GameshowPro.BgRaster.FileLifecycle.Interop;

class StaleFileCleaner
{
    internal static ImmutableArray<string> FindStaleFiles(string directory, IReadOnlySet<string> currentRunFiles)
    {
        if (!Directory.Exists(directory))
            return [];

        return [.. Directory
            .EnumerateFiles(directory, "*.png")
            .Where(f => FileNamer.IsBgRasterFile(f) && !currentRunFiles.Contains(f))];
    }

    internal static ImmutableArray<string> RecycleFiles(ImmutableArray<string> filePaths)
    {
        if (filePaths.IsEmpty)
            return [];

        // Build double-null-terminated file list for SHFileOperation
        string fileList = string.Join("\0", filePaths) + "\0\0";
        IntPtr pFrom = IntPtr.Zero;
        try
        {
            pFrom = Marshal.StringToCoTaskMemUni(fileList);

            SHFILEOPSTRUCTW fileOp = new()
            {
                hwnd = IntPtr.Zero,
                wFunc = NativeMethods.FO_DELETE,
                pFrom = pFrom,
                pTo = IntPtr.Zero,
                fFlags = NativeMethods.FOF_ALLOWUNDO | NativeMethods.FOF_NOCONFIRMATION | NativeMethods.FOF_NOERRORUI | NativeMethods.FOF_SILENT,
                fAnyOperationsAborted = 0,
                hNameMappings = IntPtr.Zero,
                lpszProgressTitle = IntPtr.Zero,
            };

            int result = NativeMethods.SHFileOperation(ref fileOp);
            if (result == 0 && fileOp.fAnyOperationsAborted == 0)
            {
                return [];
            }
            return filePaths;
        }
        finally
        {
            if (pFrom != IntPtr.Zero)
                Marshal.FreeCoTaskMem(pFrom);
        }
    }
}