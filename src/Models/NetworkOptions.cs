// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record NetworkOptions
{
    internal ImmutableArray<string> RequireAdapterType { get; init; } = Configuration.NetworkDefaults.RequireAdapterType;
    internal ImmutableArray<string> ExcludeAdapterType { get; init; } = Configuration.NetworkDefaults.ExcludeAdapterType;
    internal bool RequireUp { get; init; } = Configuration.NetworkDefaults.RequireUp;
    internal string RequireFamily { get; init; } = Configuration.NetworkDefaults.RequireFamily;
    internal ImmutableArray<string> RequireMacAddress { get; init; } = Configuration.NetworkDefaults.RequireMacAddress;
    internal ImmutableArray<string> RequireSubnet { get; init; } = Configuration.NetworkDefaults.RequireSubnet;
    internal int MinimumAddressCount { get; init; } = Configuration.NetworkDefaults.MinimumAddressCount;
    internal ImmutableArray<string> RequireName { get; init; } = Configuration.NetworkDefaults.RequireName;
    internal ImmutableArray<string> RequireDescription { get; init; } = Configuration.NetworkDefaults.RequireDescription;
    internal string IpAddressFormat { get; init; } = Configuration.NetworkDefaults.IpAddressFormat;
    internal string AdapterFormat { get; init; } = Configuration.NetworkDefaults.AdapterFormat;
    internal string TextAlign { get; init; } = Configuration.NetworkDefaults.TextAlign;
    internal string AnchorX { get; init; } = Configuration.NetworkDefaults.AnchorX;
    internal string AnchorY { get; init; } = Configuration.NetworkDefaults.AnchorY;
    internal ImmutableArray<string> X { get; init; } = Configuration.NetworkDefaults.X;
    internal ImmutableArray<string> Y { get; init; } = Configuration.NetworkDefaults.Y;
    internal ImmutableArray<string> Size { get; init; } = Configuration.NetworkDefaults.Size;
    internal ImmutableArray<string> Color { get; init; } = Configuration.NetworkDefaults.Color;
    internal bool Render { get; init; } = Configuration.NetworkDefaults.Render;
}