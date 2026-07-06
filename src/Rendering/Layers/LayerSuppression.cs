// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering.Layers;

internal static class LayerSuppression
{
    internal static bool ShouldSuppressCircle(ResolvedOptions options) =>
        options.CircleSizePx <= 0f
        || options.CircleStrokePx <= 0f
        || IsTransparent(options.CircleColor);

    internal static bool ShouldSuppressCrosshair(ResolvedOptions options) =>
        options.CrosshairLengthPx <= 0f
        || options.CrosshairStrokePx <= 0f
        || IsTransparent(options.CrosshairColor);

    internal static bool ShouldSuppressLogo(ResolvedOptions options) =>
            string.IsNullOrWhiteSpace(options.LogoSource)
            || (options.LogoWidthPx <= 0f && options.LogoHeightPx <= 0f)
            || options.LogoOpacity <= 0f;

    private static bool IsTransparent(SKColor color) => color.Alpha == 0;
}