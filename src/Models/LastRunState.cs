// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record LastRunState
{
    internal LastRunMeta Meta { get; init; } = new();
    internal ImmutableArray<OutputRecord> HardwareOutputs { get; init; } = [];
    internal GlobalOptions EffectiveConfig { get; init; } = new();
}
