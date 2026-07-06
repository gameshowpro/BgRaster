// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record LastRunMeta
{
    internal string Version { get; init; } = "";
    internal string SettingsHash { get; init; } = "";
    internal string Timestamp { get; init; } = "";
    internal FrozenDictionary<string, string> AssignedFiles { get; init; } = FrozenDictionary<string, string>.Empty;
    internal ImmutableArray<string> UnrecycledFiles { get; init; } = [];
}
