// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record CrosshairOptions
{
    internal ImmutableArray<string> X { get; init; } = ["50vw"];
    internal ImmutableArray<string> Y { get; init; } = ["50vh"];
    internal ImmutableArray<string> Length { get; init; } = ["5vmin"];
    internal ImmutableArray<string> Color { get; init; } = ["#ffffff80"];
    internal ImmutableArray<string> Stroke { get; init; } = ["1px"];
}
