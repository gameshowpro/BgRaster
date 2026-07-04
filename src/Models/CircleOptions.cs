// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record CircleOptions
{
    internal ImmutableArray<string> X { get; init; } = Configuration.CircleDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = Configuration.CircleDefaults.Y;
    internal ImmutableArray<string> Size { get; init; } = Configuration.CircleDefaults.Size;
    internal ImmutableArray<string> Color { get; init; } = Configuration.CircleDefaults.Color;
    internal ImmutableArray<string> Stroke { get; init; } = Configuration.CircleDefaults.Stroke;
}