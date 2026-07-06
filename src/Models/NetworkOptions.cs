// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record NetworkOptions
{
    internal ImmutableArray<string> RequireAdapterType { get; init; } = NetworkDefaults.RequireAdapterType;
    internal ImmutableArray<string> ExcludeAdapterType { get; init; } = NetworkDefaults.ExcludeAdapterType;
    internal bool RequireUp { get; init; } = NetworkDefaults.RequireUp;
    internal string RequireFamily { get; init; } = NetworkDefaults.RequireFamily;
    internal ImmutableArray<string> RequireMacAddress { get; init; } = NetworkDefaults.RequireMacAddress;
    internal ImmutableArray<string> RequireSubnet { get; init; } = NetworkDefaults.RequireSubnet;
    internal int MinimumAddressCount { get; init; } = NetworkDefaults.MinimumAddressCount;
    internal ImmutableArray<string> RequireName { get; init; } = NetworkDefaults.RequireName;
    internal ImmutableArray<string> RequireDescription { get; init; } = NetworkDefaults.RequireDescription;
    internal string IpAddressFormat { get; init; } = NetworkDefaults.IpAddressFormat;
    internal string AdapterFormat { get; init; } = NetworkDefaults.AdapterFormat;
    internal string TextAlign { get; init; } = NetworkDefaults.TextAlign;
    internal string AnchorX { get; init; } = NetworkDefaults.AnchorX;
    internal string AnchorY { get; init; } = NetworkDefaults.AnchorY;
    internal ImmutableArray<string> X { get; init; } = NetworkDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = NetworkDefaults.Y;
    internal ImmutableArray<string> Size { get; init; } = NetworkDefaults.Size;
    internal ImmutableArray<string> Color { get; init; } = NetworkDefaults.Color;
    internal bool Render { get; init; } = NetworkDefaults.Render;
}