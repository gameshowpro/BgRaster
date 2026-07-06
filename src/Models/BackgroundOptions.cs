// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record BackgroundOptions
{
    internal ImmutableArray<string> Color { get; init; } = BackgroundDefaults.Color;
    internal ImmutableArray<string> Image { get; init; } = BackgroundDefaults.Image;
    internal ImmutableArray<string> Fit { get; init; } = BackgroundDefaults.Fit;
    internal ImmutableArray<bool> Alternating { get; init; } = BackgroundDefaults.Alternating;
    internal ImmutableArray<bool> Border { get; init; } = BackgroundDefaults.Border;
    internal ImmutableArray<string> BorderColor { get; init; } = BackgroundDefaults.BorderColor;
}