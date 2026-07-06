// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record GridOptions
{
    internal ImmutableArray<string> Size { get; init; } = GridDefaults.Size;
    internal ImmutableArray<string> OddColor { get; init; } = GridDefaults.OddColor;
    internal ImmutableArray<string> EvenColor { get; init; } = GridDefaults.EvenColor;
    internal ImmutableArray<string> Stroke { get; init; } = GridDefaults.Stroke;
    internal ImmutableArray<string> OffsetX { get; init; } = GridDefaults.OffsetX;
    internal ImmutableArray<string> OffsetY { get; init; } = GridDefaults.OffsetY;
    internal ImmutableArray<bool> Coordinates { get; init; } = GridDefaults.Coordinates;
}