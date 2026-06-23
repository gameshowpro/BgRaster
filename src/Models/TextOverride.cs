// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record TextOverride
{
    internal ImmutableArray<string>? Text { get; init; }
    internal ImmutableArray<string>? Size { get; init; }
    internal ImmutableArray<string>? Color { get; init; }
    internal string? X { get; init; }
    internal string? Y { get; init; }
}
