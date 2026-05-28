namespace GameshowPro.BgRaster.Discovery;

using GameshowPro.BgRaster.Discovery.Interop;

sealed class DisplayDiscovery : IDisplayDiscovery
{
    public unsafe HardwareProfile Discover()
    {
        ImmutableArray<TargetName> targetNames = QueryTargetNames();
        List<OutputRecord> records = [];

        DISPLAY_DEVICE adapter = default;
        adapter.cb = (uint)sizeof(DISPLAY_DEVICE);

        for (uint adapterIdx = 0; DisplayInterop.EnumDisplayDevices(null, adapterIdx, ref adapter, 0); adapterIdx++)
        {
            uint flags = adapter.StateFlags;
            if ((flags & DisplayConstants.DISPLAY_DEVICE_MIRRORING_DRIVER) != 0)
                continue;
            if ((flags & DisplayConstants.DISPLAY_DEVICE_ATTACHED_TO_DESKTOP) == 0)
                continue;

            string adapterDeviceName = ReadFixedChars(adapter.DeviceName, 32);
            string adapterFriendly = ReadFixedChars(adapter.DeviceString, 128);

            DEVMODE devMode = default;
            devMode.dmSize = (ushort)sizeof(DEVMODE);
            if (!DisplayInterop.EnumDisplaySettingsEx(adapterDeviceName, DisplayConstants.ENUM_CURRENT_SETTINGS, ref devMode, 0))
                continue;

            int desktopX = devMode.dmPosition.x;
            int desktopY = devMode.dmPosition.y;
            int widthPx = (int)devMode.dmPelsWidth;
            int heightPx = (int)devMode.dmPelsHeight;
            int refresh = (int)devMode.dmDisplayFrequency;
            int rotationDeg = devMode.dmDisplayOrientation switch
            {
                DisplayConstants.DMDO_90 => 90,
                DisplayConstants.DMDO_180 => 180,
                DisplayConstants.DMDO_270 => 270,
                _ => 0,
            };

            (int dpiX, int dpiY) = QueryDpi(desktopX, desktopY);

            DISPLAY_DEVICE monitor = default;
            monitor.cb = (uint)sizeof(DISPLAY_DEVICE);

            for (uint monitorIdx = 0;
                 DisplayInterop.EnumDisplayDevices(adapterDeviceName, monitorIdx, ref monitor, DisplayConstants.EDD_GET_DEVICE_INTERFACE_NAME);
                 monitorIdx++)
            {
                if ((monitor.StateFlags & DisplayConstants.DISPLAY_DEVICE_ACTIVE) == 0)
                    continue;

                string deviceId = ReadFixedChars(monitor.DeviceID, 128);
                string monitorString = ReadFixedChars(monitor.DeviceString, 128);

                string friendlyName = LookupFriendlyName(targetNames, deviceId, monitorString);

                records.Add(new OutputRecord
                {
                    Id = deviceId,
                    DesktopX = desktopX,
                    DesktopY = desktopY,
                    WidthPx = widthPx,
                    HeightPx = heightPx,
                    DpiX = dpiX,
                    DpiY = dpiY,
                    Rotation = rotationDeg,
                    RefreshRateHz = refresh,
                    AdapterName = adapterDeviceName,
                    FriendlyName = friendlyName,
                });
            }
        }

        OutputRecord[] sorted = [.. records
            .OrderBy(r => r.DesktopY)
            .ThenBy(r => r.DesktopX)
            .ThenBy(r => r.Id, StringComparer.Ordinal)
            .Select((r, i) => r with { Index = i })];

        if (sorted.Length == 0)
        {
            Console.WriteLine("DisplayDiscovery: no active displays found.");
            return new HardwareProfile([]);
        }

        return new HardwareProfile([.. sorted]);
    }

    static unsafe (int dpiX, int dpiY) QueryDpi(int desktopX, int desktopY)
    {
        POINT pt = new() { x = desktopX, y = desktopY };
        nint hmon = DisplayInterop.MonitorFromPoint(pt, DisplayConstants.MONITOR_DEFAULTTONEAREST);
        if (hmon == 0)
            return (96, 96);
        if (DisplayInterop.GetDpiForMonitor(hmon, DisplayConstants.MDT_EFFECTIVE_DPI, out uint dx, out uint dy) != 0)
            return (96, 96);
        return ((int)dx, (int)dy);
    }

    readonly record struct TargetName(LUID AdapterId, uint TargetId, string DevicePath, string FriendlyName);

    static unsafe ImmutableArray<TargetName> QueryTargetNames()
    {
        if (DisplayInterop.GetDisplayConfigBufferSizes(
                DisplayConstants.QDC_ONLY_ACTIVE_PATHS,
                out uint numPaths, out uint numModes) != 0)
            return [];

        if (numPaths == 0)
            return [];

        DISPLAYCONFIG_PATH_INFO[] paths = new DISPLAYCONFIG_PATH_INFO[numPaths];
        DISPLAYCONFIG_MODE_INFO[] modes = new DISPLAYCONFIG_MODE_INFO[numModes];

        int rc;
        fixed (DISPLAYCONFIG_PATH_INFO* pPaths = paths)
        fixed (DISPLAYCONFIG_MODE_INFO* pModes = modes)
        {
            rc = DisplayInterop.QueryDisplayConfig(
                DisplayConstants.QDC_ONLY_ACTIVE_PATHS,
                &numPaths, pPaths, &numModes, pModes, null);
        }

        if (rc != 0)
            return [];

        ImmutableArray<TargetName>.Builder builder = ImmutableArray.CreateBuilder<TargetName>((int)numPaths);
        for (int i = 0; i < (int)numPaths; i++)
        {
            DISPLAYCONFIG_TARGET_DEVICE_NAME req = default;
            req.header.type = DisplayConstants.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
            req.header.size = (uint)sizeof(DISPLAYCONFIG_TARGET_DEVICE_NAME);
            req.header.adapterId = paths[i].targetInfo.adapterId;
            req.header.id = paths[i].targetInfo.id;

            int hr = DisplayInterop.DisplayConfigGetDeviceInfo(&req);
            if (hr != 0)
                continue;

            string devicePath = ReadFixedChars(req.monitorDevicePath, 128);
            string friendly = ReadFixedChars(req.monitorFriendlyDeviceName, 64);

            builder.Add(new TargetName(req.header.adapterId, req.header.id, devicePath, friendly));
        }

        return builder.ToImmutable();
    }

    static string LookupFriendlyName(ImmutableArray<TargetName> targets, string monitorDeviceId, string fallback)
    {
        foreach (TargetName t in targets)
        {
            if (string.Equals(t.DevicePath, monitorDeviceId, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(t.FriendlyName))
                return t.FriendlyName;
        }
        return fallback;
    }

    static unsafe string ReadFixedChars(char* p, int maxLen)
    {
        int len = 0;
        while (len < maxLen && p[len] != '\0')
            len++;
        return new string(p, 0, len);
    }
}
