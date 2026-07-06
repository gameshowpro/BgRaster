// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Network;

/// <summary>Collected representation of a single IP address on an adapter.</summary>
internal record AdapterIpAddress
{
    internal string Address { get; init; } = string.Empty;
    internal int CidrBits { get; init; }
    internal string Origin { get; init; } = string.Empty;
}

/// <summary>Collected representation of a network adapter.</summary>
internal record AdapterInfo
{
    internal string Name { get; init; } = string.Empty;
    internal string Description { get; init; } = string.Empty;
    internal string Id { get; init; } = string.Empty;
    internal string Type { get; init; } = string.Empty;
    internal string TypeLong { get; init; } = string.Empty;
    internal string Status { get; init; } = string.Empty;
    internal string Speed { get; init; } = string.Empty;
    internal string MacAddress { get; init; } = string.Empty;
    internal ImmutableArray<AdapterIpAddress> IpAddresses { get; init; } = [];
}
