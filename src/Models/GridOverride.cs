// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record GridOverride
{
    internal string? Size { get; init; }
    internal string? OddColor { get; init; }
    internal string? EvenColor { get; init; }
    internal string? Stroke { get; init; }
    internal string? OffsetX { get; init; }
    internal string? OffsetY { get; init; }
    internal bool? Coordinates { get; init; }
}
