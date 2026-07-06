// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record TextOverride
{
    internal ImmutableArray<string>? Format { get; init; }
    internal string? TextAlign { get; init; }
    internal string? AnchorX { get; init; }
    internal string? AnchorY { get; init; }
    internal ImmutableArray<string>? Size { get; init; }
    internal ImmutableArray<string>? Color { get; init; }
    internal string? X { get; init; }
    internal string? Y { get; init; }
}
