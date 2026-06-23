// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Wallpaper.Interop;

internal static class WallpaperInterop
{
    internal static readonly Guid ClsidDesktopWallpaper = new("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD");
    internal static readonly Guid IidIDesktopWallpaper = new("B92B56A9-8B55-4E14-9A89-0199BBB6F93B");

    internal const int CLSCTX_INPROC_SERVER = 0x1;
    internal const int CLSCTX_LOCAL_SERVER = 0x4;
    internal const int COINIT_APARTMENTTHREADED = 0x2;
    internal const int COINIT_DISABLE_OLE1DDE = 0x4;
    internal const int RPC_E_CHANGED_MODE = unchecked((int)0x80010106);
    internal const int S_OK = 0;
    internal const int S_FALSE = 1;

    // IDesktopWallpaper vtable indexes (after IUnknown: 0=QueryInterface, 1=AddRef, 2=Release)
    internal const int VT_SetWallpaper = 3;
    internal const int VT_GetWallpaper = 4;
    internal const int VT_GetMonitorDevicePathAt = 5;
    internal const int VT_GetMonitorDevicePathCount = 6;
    internal const int VT_Release = 2;
}

internal static partial class Ole32
{
    [LibraryImport("ole32.dll")]
    internal static unsafe partial int CoCreateInstance(
        Guid* rclsid, nint pUnkOuter, int dwClsContext, Guid* riid, out nint ppv);

    [LibraryImport("ole32.dll")]
    internal static partial int CoInitializeEx(nint reserved, int coInit);

    [LibraryImport("ole32.dll")]
    internal static partial void CoUninitialize();

    [LibraryImport("ole32.dll")]
    internal static partial void CoTaskMemFree(nint pv);
}
