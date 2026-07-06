// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal sealed record RunStatus
{
    internal FrozenDictionary<string, string> HardwareStatuses { get; init; } = FrozenDictionary<string, string>.Empty;
    internal ImmutableArray<ConfiguredOutputStatus> ConfiguredOutputs { get; init; } = [];
}

internal sealed record ConfiguredOutputStatus
{
    internal string TargetDescription { get; init; } = "";
    internal string Status { get; init; } = "";
    internal string? Reason { get; init; }
    internal ImmutableArray<SliceStatus> Slices { get; init; } = [];
}

internal sealed record SliceStatus
{
    internal string Status { get; init; } = "";
    internal string? Reason { get; init; }
}
