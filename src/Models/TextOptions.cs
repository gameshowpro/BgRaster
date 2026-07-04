// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record TextOptions
{
    internal ImmutableArray<string> Format { get; init; } = Configuration.TextDefaults.Format;
    internal string TextAlign { get; init; } = Configuration.TextDefaults.TextAlign;
    internal string AnchorX { get; init; } = Configuration.TextDefaults.AnchorX;
    internal string AnchorY { get; init; } = Configuration.TextDefaults.AnchorY;
    internal ImmutableArray<string> Size { get; init; } = Configuration.TextDefaults.Size;
    internal ImmutableArray<string> Color { get; init; } = Configuration.TextDefaults.Color;
    internal ImmutableArray<string> X { get; init; } = Configuration.TextDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = Configuration.TextDefaults.Y;
}