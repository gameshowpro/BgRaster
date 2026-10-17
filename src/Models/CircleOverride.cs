// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record CircleOverride
{
    internal string? X { get; init; }
    internal string? Y { get; init; }
    internal string? Size { get; init; }
    internal string? Color { get; init; }
    internal string? Stroke { get; init; }
}
