// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record LogoOptions
{
    internal ImmutableArray<string> Source { get; init; } = Configuration.LogoDefaults.Source;
    internal ImmutableArray<string> X { get; init; } = Configuration.LogoDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = Configuration.LogoDefaults.Y;
    internal string AnchorX { get; init; } = Configuration.LogoDefaults.AnchorX;
    internal string AnchorY { get; init; } = Configuration.LogoDefaults.AnchorY;
    internal ImmutableArray<string> Width { get; init; } = Configuration.LogoDefaults.Width;
    internal ImmutableArray<string> Height { get; init; } = Configuration.LogoDefaults.Height;
    internal ImmutableArray<float> Opacity { get; init; } = Configuration.LogoDefaults.Opacity;
}