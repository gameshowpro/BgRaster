// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record CircleOptions
{
    internal ImmutableArray<string> X { get; init; } = CircleDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = CircleDefaults.Y;
    internal ImmutableArray<string> Size { get; init; } = CircleDefaults.Size;
    internal ImmutableArray<string> Color { get; init; } = CircleDefaults.Color;
    internal ImmutableArray<string> Stroke { get; init; } = CircleDefaults.Stroke;
}