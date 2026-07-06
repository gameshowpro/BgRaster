// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record LogoOverride
{
    internal string? Source { get; init; }
    internal string? X { get; init; }
    internal string? Y { get; init; }
    internal string? AnchorX { get; init; }
    internal string? AnchorY { get; init; }
    internal string? Width { get; init; }
    internal string? Height { get; init; }
    internal float? Opacity { get; init; }
}
