// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Discovery.Interop;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct DISPLAY_DEVICE
{
    internal uint cb;
    internal fixed char DeviceName[32];
    internal fixed char DeviceString[128];
    internal uint StateFlags;
    internal fixed char DeviceID[128];
    internal fixed char DeviceKey[128];
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    internal int x;
    internal int y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    internal int left;
    internal int top;
    internal int right;
    internal int bottom;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct DEVMODE
{
    internal fixed char dmDeviceName[32];
    internal ushort dmSpecVersion;
    internal ushort dmDriverVersion;
    internal ushort dmSize;
    internal ushort dmDriverExtra;
    internal uint dmFields;
    internal POINT dmPosition;
    internal uint dmDisplayOrientation;
    internal uint dmDisplayFixedOutput;
    internal short dmColor;
    internal short dmDuplex;
    internal short dmYResolution;
    internal short dmTTOption;
    internal short dmCollate;
    internal fixed char dmFormName[32];
    internal ushort dmLogPixels;
    internal uint dmBitsPerPel;
    internal uint dmPelsWidth;
    internal uint dmPelsHeight;
    internal uint dmDisplayFlags;
    internal uint dmDisplayFrequency;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LUID
{
    internal uint LowPart;
    internal int HighPart;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_PATH_SOURCE_INFO
{
    internal LUID adapterId;
    internal uint id;
    internal uint modeInfoIdx;
    internal uint statusFlags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_PATH_TARGET_INFO
{
    internal LUID adapterId;
    internal uint id;
    internal uint modeInfoIdx;
    internal uint outputTechnology;
    internal uint rotation;
    internal uint scaling;
    internal DISPLAYCONFIG_RATIONAL refreshRate;
    internal uint scanLineOrdering;
    internal int targetAvailable;
    internal uint statusFlags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_RATIONAL
{
    internal uint Numerator;
    internal uint Denominator;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_PATH_INFO
{
    internal DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
    internal DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
    internal uint flags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_2DREGION
{
    internal uint cx;
    internal uint cy;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
{
    internal ulong pixelRate;
    internal DISPLAYCONFIG_RATIONAL hSyncFreq;
    internal DISPLAYCONFIG_RATIONAL vSyncFreq;
    internal DISPLAYCONFIG_2DREGION activeSize;
    internal DISPLAYCONFIG_2DREGION totalSize;
    internal uint videoStandard;
    internal uint scanLineOrdering;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_TARGET_MODE
{
    internal DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINTL
{
    internal int x;
    internal int y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_SOURCE_MODE
{
    internal uint width;
    internal uint height;
    internal uint pixelFormat;
    internal POINTL position;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_DESKTOP_IMAGE_INFO
{
    internal POINTL PathSourceSize;
    internal RECT DesktopImageRegion;
    internal RECT DesktopImageClip;
}

[StructLayout(LayoutKind.Explicit)]
internal struct DISPLAYCONFIG_MODE_INFO_UNION
{
    [FieldOffset(0)] internal DISPLAYCONFIG_TARGET_MODE targetMode;
    [FieldOffset(0)] internal DISPLAYCONFIG_SOURCE_MODE sourceMode;
    [FieldOffset(0)] internal DISPLAYCONFIG_DESKTOP_IMAGE_INFO desktopImageInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_MODE_INFO
{
    internal uint infoType;
    internal uint id;
    internal LUID adapterId;
    internal DISPLAYCONFIG_MODE_INFO_UNION modeUnion;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_DEVICE_INFO_HEADER
{
    internal uint type;
    internal uint size;
    internal LUID adapterId;
    internal uint id;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct DISPLAYCONFIG_TARGET_DEVICE_NAME
{
    internal DISPLAYCONFIG_DEVICE_INFO_HEADER header;
    internal uint flags;
    internal uint outputTechnology;
    internal ushort edidManufactureId;
    internal ushort edidProductCodeId;
    internal uint connectorInstance;
    internal fixed char monitorFriendlyDeviceName[64];
    internal fixed char monitorDevicePath[128];
}

internal static class DisplayConstants
{
    internal const uint ENUM_CURRENT_SETTINGS = 0xFFFFFFFFu;
    internal const uint EDD_GET_DEVICE_INTERFACE_NAME = 0x00000001;
    internal const uint DISPLAY_DEVICE_ACTIVE = 0x00000001;
    internal const uint DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001;
    internal const uint DISPLAY_DEVICE_MIRRORING_DRIVER = 0x00000008;

    internal const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;

    internal const uint DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2;

    internal const int MDT_EFFECTIVE_DPI = 0;
    internal const uint MONITOR_DEFAULTTONEAREST = 2;

    internal const int DMDO_DEFAULT = 0;
    internal const int DMDO_90 = 1;
    internal const int DMDO_180 = 2;
    internal const int DMDO_270 = 3;
}
