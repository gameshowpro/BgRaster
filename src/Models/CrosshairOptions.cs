// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record CrosshairOptions
{
    internal ImmutableArray<string> X { get; init; } = Configuration.CrosshairDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = Configuration.CrosshairDefaults.Y;
    internal ImmutableArray<string> Length { get; init; } = Configuration.CrosshairDefaults.Length;
    internal ImmutableArray<string> Color { get; init; } = Configuration.CrosshairDefaults.Color;
    internal ImmutableArray<string> Stroke { get; init; } = Configuration.CrosshairDefaults.Stroke;
}