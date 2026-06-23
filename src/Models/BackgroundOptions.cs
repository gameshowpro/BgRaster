// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record BackgroundOptions
{
    internal ImmutableArray<string> Color { get; init; } = ["#FF0000", "#00FF00", "#0000FF"];
    internal ImmutableArray<string> Image { get; init; } = [""];
    internal ImmutableArray<string> Fit { get; init; } = ["CropToFill"];
    internal ImmutableArray<bool> Alternating { get; init; } = [false];
    internal ImmutableArray<bool> Border { get; init; } = [false];
    internal ImmutableArray<string> BorderColor { get; init; } = ["#FFFFFF"];
}
