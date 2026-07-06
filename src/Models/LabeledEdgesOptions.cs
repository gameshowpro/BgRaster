// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record LabeledEdgesOptions
{
    internal ImmutableArray<string> TextSize { get; init; } = LabeledEdgesDefaults.TextSize;
    internal ImmutableArray<string> TailLength { get; init; } = LabeledEdgesDefaults.TailLength;
    internal ImmutableArray<string> Thickness { get; init; } = LabeledEdgesDefaults.Thickness;
    internal ImmutableArray<float> HeadScale { get; init; } = LabeledEdgesDefaults.HeadScale;
    internal ImmutableArray<LabeledEdgesScope> Scope { get; init; } = LabeledEdgesDefaults.Scope;
    internal ImmutableArray<LabeledEdgeSide> Side { get; init; } = LabeledEdgesDefaults.Side;
}