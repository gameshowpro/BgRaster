// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record NetworkOverride
{
    internal ImmutableArray<string>? RequireAdapterType { get; init; }
    internal ImmutableArray<string>? ExcludeAdapterType { get; init; }
    internal bool? RequireUp { get; init; }
    internal string? RequireFamily { get; init; }
    internal ImmutableArray<string>? RequireMacAddress { get; init; }
    internal ImmutableArray<string>? RequireSubnet { get; init; }
    internal int? MinimumAddressCount { get; init; }
    internal ImmutableArray<string>? RequireName { get; init; }
    internal ImmutableArray<string>? RequireDescription { get; init; }
    internal string? IpAddressFormat { get; init; }
    internal string? AdapterFormat { get; init; }
    internal string? TextAlign { get; init; }
    internal string? AnchorX { get; init; }
    internal string? AnchorY { get; init; }
    internal ImmutableArray<string>? X { get; init; }
    internal ImmutableArray<string>? Y { get; init; }
    internal ImmutableArray<string>? Size { get; init; }
    internal ImmutableArray<string>? Color { get; init; }
    internal bool? Render { get; init; }
}
