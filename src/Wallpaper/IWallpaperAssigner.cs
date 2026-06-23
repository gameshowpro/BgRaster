// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Wallpaper;

interface IWallpaperAssigner
{
    Task<bool> AssignAsync(FrozenDictionary<string, string> outputIdToFilePath);
    Task ClearAsync(IReadOnlyCollection<string> outputIds);
}
