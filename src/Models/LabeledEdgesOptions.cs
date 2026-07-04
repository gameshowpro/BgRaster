// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record LabeledEdgesOptions
{
    internal ImmutableArray<string> TextSize { get; init; } = Configuration.LabeledEdgesDefaults.TextSize;
    internal ImmutableArray<string> TailLength { get; init; } = Configuration.LabeledEdgesDefaults.TailLength;
    internal ImmutableArray<string> Thickness { get; init; } = Configuration.LabeledEdgesDefaults.Thickness;
    internal ImmutableArray<float> HeadScale { get; init; } = Configuration.LabeledEdgesDefaults.HeadScale;
    internal ImmutableArray<LabeledEdgesScope> Scope { get; init; } = Configuration.LabeledEdgesDefaults.Scope;
    internal ImmutableArray<LabeledEdgeSide> Side { get; init; } = Configuration.LabeledEdgesDefaults.Side;
}