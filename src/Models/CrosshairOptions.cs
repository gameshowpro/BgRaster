// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record CrosshairOptions
{
    internal ImmutableArray<string> X { get; init; } = CrosshairDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = CrosshairDefaults.Y;
    internal ImmutableArray<string> Length { get; init; } = CrosshairDefaults.Length;
    internal ImmutableArray<string> Color { get; init; } = CrosshairDefaults.Color;
    internal ImmutableArray<string> Stroke { get; init; } = CrosshairDefaults.Stroke;
}