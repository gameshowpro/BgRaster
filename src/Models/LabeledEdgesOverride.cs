// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record LabeledEdgesOverride
{
    internal string? TextSize { get; init; }
    internal string? TailLength { get; init; }
    internal string? Thickness { get; init; }
    internal float? HeadScale { get; init; }
    internal string? Scope { get; init; }
    internal ImmutableArray<LabeledEdgeSide>? Side { get; init; }
}