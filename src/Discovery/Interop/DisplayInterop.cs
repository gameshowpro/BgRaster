// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Discovery.Interop;

internal static partial class DisplayInterop
{
    [LibraryImport("user32.dll", EntryPoint = "EnumDisplayDevicesW",
        StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EnumDisplayDevices(
        string? lpDevice, uint iDevNum,
        ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [LibraryImport("user32.dll", EntryPoint = "EnumDisplaySettingsExW",
        StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EnumDisplaySettingsEx(
        string lpszDeviceName, uint iModeNum,
        ref DEVMODE lpDevMode, uint dwFlags);

    [LibraryImport("shcore.dll")]
    internal static partial int GetDpiForMonitor(
        nint hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    [LibraryImport("user32.dll")]
    internal static partial nint MonitorFromPoint(POINT pt, uint dwFlags);

    [LibraryImport("user32.dll")]
    internal static partial int GetDisplayConfigBufferSizes(
        uint flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);

    [LibraryImport("user32.dll")]
    internal static unsafe partial int QueryDisplayConfig(
        uint flags,
        uint* numPathArrayElements,
        DISPLAYCONFIG_PATH_INFO* pathArray,
        uint* numModeInfoArrayElements,
        DISPLAYCONFIG_MODE_INFO* modeInfoArray,
        void* currentTopologyId);

    [LibraryImport("user32.dll", EntryPoint = "DisplayConfigGetDeviceInfo")]
    internal static unsafe partial int DisplayConfigGetDeviceInfo(
        DISPLAYCONFIG_TARGET_DEVICE_NAME* requestPacket);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetProcessDPIAware();
}
