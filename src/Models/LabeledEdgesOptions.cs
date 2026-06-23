// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

record LabeledEdgesOptions
{
    internal ImmutableArray<string> TextSize { get; init; } = ["10px"];
    internal ImmutableArray<string> TailLength { get; init; } = ["10px"];
    internal ImmutableArray<string> Thickness { get; init; } = ["3px"];
    internal ImmutableArray<float> HeadScale { get; init; } = [1f];
    internal ImmutableArray<LabeledEdgesScope> Scope { get; init; } = [LabeledEdgesScope.Desktop];
    internal ImmutableArray<LabeledEdgeSide> Side { get; init; } =
    [
        LabeledEdgeSide.TL,
        LabeledEdgeSide.T,
        LabeledEdgeSide.TR,
        LabeledEdgeSide.R,
        LabeledEdgeSide.BR,
        LabeledEdgeSide.B,
        LabeledEdgeSide.BL,
        LabeledEdgeSide.L,
    ];
}