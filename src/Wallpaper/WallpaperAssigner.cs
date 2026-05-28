namespace GameshowPro.BgRaster.Wallpaper;

using GameshowPro.BgRaster.Wallpaper.Interop;

sealed class WallpaperAssigner : IWallpaperAssigner
{
    public Task<bool> AssignAsync(FrozenDictionary<string, string> outputIdToFilePath)
    {
        if (outputIdToFilePath.Count == 0)
            return Task.FromResult(true);

        bool allOk = true;
        bool comOk = WithDesktopWallpaper(instance =>
        {
            foreach (KeyValuePair<string, string> kv in outputIdToFilePath)
            {
                int hr = SetWallpaper(instance, kv.Key, kv.Value);
                if (hr != WallpaperInterop.S_OK)
                {
                    Console.Error.WriteLine($"WallpaperAssigner: SetWallpaper failed for '{kv.Key}' (HRESULT 0x{hr:X8}).");
                    allOk = false;
                }
            }
        });

        return Task.FromResult(comOk && allOk);
    }

    public Task ClearAsync(IReadOnlyCollection<string> outputIds)
    {
        if (outputIds.Count == 0)
            return Task.CompletedTask;

        WithDesktopWallpaper(instance =>
        {
            foreach (string id in outputIds)
            {
                int hr = SetWallpaper(instance, id, "");
                if (hr != WallpaperInterop.S_OK)
                    Console.Error.WriteLine($"WallpaperAssigner: clear failed for '{id}' (HRESULT 0x{hr:X8}).");
            }
        });

        return Task.CompletedTask;
    }

    static unsafe bool WithDesktopWallpaper(Action<nint> body)
    {
        int initHr = Ole32.CoInitializeEx(0,
            WallpaperInterop.COINIT_APARTMENTTHREADED | WallpaperInterop.COINIT_DISABLE_OLE1DDE);
        bool needsUninit = initHr == WallpaperInterop.S_OK || initHr == WallpaperInterop.S_FALSE;

        try
        {
            Guid clsid = WallpaperInterop.ClsidDesktopWallpaper;
            Guid iid = WallpaperInterop.IidIDesktopWallpaper;
            int hr = Ole32.CoCreateInstance(&clsid, 0, WallpaperInterop.CLSCTX_LOCAL_SERVER, &iid, out nint instance);
            if (hr != WallpaperInterop.S_OK || instance == 0)
            {
                Console.Error.WriteLine($"WallpaperAssigner: CoCreateInstance(IDesktopWallpaper) failed (HRESULT 0x{hr:X8}).");
                return false;
            }

            try
            {
                body(instance);
                return true;
            }
            finally
            {
                Release(instance);
            }
        }
        finally
        {
            if (needsUninit)
                Ole32.CoUninitialize();
        }
    }

    static unsafe int SetWallpaper(nint instance, string monitorId, string wallpaperPath)
    {
        nint vtable = *(nint*)instance;
        nint slot = ((nint*)vtable)[WallpaperInterop.VT_SetWallpaper];
        delegate* unmanaged[Stdcall]<nint, char*, char*, int> setWallpaper =
            (delegate* unmanaged[Stdcall]<nint, char*, char*, int>)slot;

        fixed (char* pMon = monitorId)
        fixed (char* pPath = wallpaperPath)
        {
            return setWallpaper(instance, pMon, pPath);
        }
    }

    static unsafe void Release(nint instance)
    {
        nint vtable = *(nint*)instance;
        nint slot = ((nint*)vtable)[WallpaperInterop.VT_Release];
        delegate* unmanaged[Stdcall]<nint, uint> release =
            (delegate* unmanaged[Stdcall]<nint, uint>)slot;
        release(instance);
    }
}
