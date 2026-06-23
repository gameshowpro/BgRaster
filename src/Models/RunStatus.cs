// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

sealed record RunStatus
{
    internal FrozenDictionary<string, string> HardwareStatuses { get; init; } = FrozenDictionary<string, string>.Empty;
    internal ImmutableArray<ConfiguredOutputStatus> ConfiguredOutputs { get; init; } = [];
}

sealed record ConfiguredOutputStatus
{
    internal string TargetDescription { get; init; } = "";
    internal string Status { get; init; } = "";
    internal string? Reason { get; init; }
    internal ImmutableArray<SliceStatus> Slices { get; init; } = [];
}

sealed record SliceStatus
{
    internal string Status { get; init; } = "";
    internal string? Reason { get; init; }
}
