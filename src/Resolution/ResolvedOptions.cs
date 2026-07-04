// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Resolution;

record ResolvedOptions
{
    internal ImmutableArray<string> TextLines { get; init; } = [];
    internal ImmutableArray<float> TextSizesPx { get; init; } = [];
    internal ImmutableArray<SKColor> TextColors { get; init; } = [];
    internal string TextTextAlign { get; init; } = "center";
    internal string TextAnchorX { get; init; } = "center";
    internal string TextAnchorY { get; init; } = "center";
    internal float TextXPx { get; init; }
    internal float TextYPx { get; init; }
    internal SKColor BackgroundColor { get; init; }
    internal string BackgroundImage { get; init; } = "";
    internal FitMode BackgroundFit { get; init; } = FitMode.CropToFill;
    internal bool Alternating { get; init; }
    internal bool Border { get; init; }
    internal SKColor BorderColor { get; init; }
    internal float GridSizePx { get; init; }
    internal SKColor GridOddColor { get; init; }
    internal SKColor GridEvenColor { get; init; }
    internal float GridStrokePx { get; init; }
    internal float GridOffsetXPx { get; init; }
    internal float GridOffsetYPx { get; init; }
    internal bool GridCoordinates { get; init; }
    internal float CircleXPx { get; init; }
    internal float CircleYPx { get; init; }
    internal float CircleSizePx { get; init; }
    internal SKColor CircleColor { get; init; }
    internal float CircleStrokePx { get; init; }
    internal float CrosshairXPx { get; init; }
    internal float CrosshairYPx { get; init; }
    internal float CrosshairLengthPx { get; init; }
    internal SKColor CrosshairColor { get; init; }
    internal float CrosshairStrokePx { get; init; }
    internal float LabeledEdgesTextSizePx { get; init; }
    internal float LabeledEdgesTailLengthPx { get; init; }
    internal float LabeledEdgesThicknessPx { get; init; }
    internal float LabeledEdgesHeadScale { get; init; } = 1f;
    internal LabeledEdgesScope LabeledEdgesScope { get; init; } = LabeledEdgesScope.Output;
    internal float LabeledEdgesScopeWidthPx { get; init; }
    internal float LabeledEdgesScopeHeightPx { get; init; }
    internal ImmutableArray<LabeledEdgeSide> LabeledEdgesSides { get; init; } = [];
    internal string LogoSource { get; init; } = "";
    internal float LogoXPx { get; init; }
    internal float LogoYPx { get; init; }
    internal string LogoAnchorX { get; init; } = "center";
    internal string LogoAnchorY { get; init; } = "center";
    internal float LogoWidthPx { get; init; }
    internal float LogoHeightPx { get; init; }
    internal float LogoOpacity { get; init; } = 1f;
    internal ImmutableArray<AdapterInfo> NetworkAdapters { get; init; } = [];
    internal Models.NetworkOptions NetworkOptions { get; init; } = new();
    internal ImmutableArray<float> NetworkSizesPx { get; init; } = [];
    internal ImmutableArray<SKColor> NetworkColors { get; init; } = [];
    internal string NetworkTextAlign { get; init; } = "start";
    internal string NetworkAnchorX { get; init; } = "left";
    internal string NetworkAnchorY { get; init; } = "center";
    internal float NetworkXPx { get; init; }
    internal float NetworkYPx { get; init; }
}
