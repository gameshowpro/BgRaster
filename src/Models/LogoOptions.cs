// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record LogoOptions
{
    internal ImmutableArray<string> Source { get; init; } = LogoDefaults.Source;
    internal ImmutableArray<string> X { get; init; } = LogoDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = LogoDefaults.Y;
    internal string AnchorX { get; init; } = LogoDefaults.AnchorX;
    internal string AnchorY { get; init; } = LogoDefaults.AnchorY;
    internal ImmutableArray<string> Width { get; init; } = LogoDefaults.Width;
    internal ImmutableArray<string> Height { get; init; } = LogoDefaults.Height;
    internal ImmutableArray<float> Opacity { get; init; } = LogoDefaults.Opacity;
}