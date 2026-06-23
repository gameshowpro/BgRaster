// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record CrosshairOverride
{
    internal string? X { get; init; }
    internal string? Y { get; init; }
    internal string? Length { get; init; }
    internal string? Color { get; init; }
    internal string? Stroke { get; init; }
}
