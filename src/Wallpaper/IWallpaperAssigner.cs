// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Wallpaper;

internal interface IWallpaperAssigner
{
    Task<bool> AssignAsync(FrozenDictionary<string, string> outputIdToFilePath);
    Task ClearAsync(IReadOnlyCollection<string> outputIds);
}
