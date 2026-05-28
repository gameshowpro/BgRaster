namespace GameshowPro.BgRaster.Resolution;

record ResolvedOptions
{
    internal ImmutableArray<string> TextLines { get; init; } = [];
    internal ImmutableArray<float> TextSizesPx { get; init; } = [];
    internal ImmutableArray<SKColor> TextColors { get; init; } = [];
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
    internal float CircleSizePx { get; init; }
    internal SKColor CircleColor { get; init; }
    internal float CircleStrokePx { get; init; }
    internal float CrosshairLengthPx { get; init; }
    internal SKColor CrosshairColor { get; init; }
    internal float CrosshairStrokePx { get; init; }
    internal string LogoSource { get; init; } = "";
    internal float LogoXPx { get; init; }
    internal float LogoYPx { get; init; }
    internal float LogoWidthPx { get; init; }
    internal float LogoHeightPx { get; init; }
    internal float LogoOpacity { get; init; } = 1f;
}
