// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record BackgroundOptions
{
    internal ImmutableArray<string> Color { get; init; } = Configuration.BackgroundDefaults.Color;
    internal ImmutableArray<string> Image { get; init; } = Configuration.BackgroundDefaults.Image;
    internal ImmutableArray<string> Fit { get; init; } = Configuration.BackgroundDefaults.Fit;
    internal ImmutableArray<bool> Alternating { get; init; } = Configuration.BackgroundDefaults.Alternating;
    internal ImmutableArray<bool> Border { get; init; } = Configuration.BackgroundDefaults.Border;
    internal ImmutableArray<string> BorderColor { get; init; } = Configuration.BackgroundDefaults.BorderColor;
}