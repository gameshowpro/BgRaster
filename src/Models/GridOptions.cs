// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record GridOptions
{
    internal ImmutableArray<string> Size { get; init; } = Configuration.GridDefaults.Size;
    internal ImmutableArray<string> OddColor { get; init; } = Configuration.GridDefaults.OddColor;
    internal ImmutableArray<string> EvenColor { get; init; } = Configuration.GridDefaults.EvenColor;
    internal ImmutableArray<string> Stroke { get; init; } = Configuration.GridDefaults.Stroke;
    internal ImmutableArray<string> OffsetX { get; init; } = Configuration.GridDefaults.OffsetX;
    internal ImmutableArray<string> OffsetY { get; init; } = Configuration.GridDefaults.OffsetY;
    internal ImmutableArray<bool> Coordinates { get; init; } = Configuration.GridDefaults.Coordinates;
}