// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record TextOptions
{
    internal ImmutableArray<string> Format { get; init; } = TextDefaults.Format;
    internal string TextAlign { get; init; } = TextDefaults.TextAlign;
    internal string AnchorX { get; init; } = TextDefaults.AnchorX;
    internal string AnchorY { get; init; } = TextDefaults.AnchorY;
    internal ImmutableArray<string> Size { get; init; } = TextDefaults.Size;
    internal ImmutableArray<string> Color { get; init; } = TextDefaults.Color;
    internal ImmutableArray<string> X { get; init; } = TextDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = TextDefaults.Y;
}