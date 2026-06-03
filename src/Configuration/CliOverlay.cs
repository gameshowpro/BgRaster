namespace GameshowPro.BgRaster.Configuration;

record CliOverlay
{
    internal string? Text { get; init; }
    internal string? TextSize { get; init; }
    internal string? TextColor { get; init; }
    internal string? TextX { get; init; }
    internal string? TextY { get; init; }
    internal string? BackgroundColor { get; init; }
    internal string? BackgroundImage { get; init; }
    internal string? BackgroundFit { get; init; }
    internal bool? BackgroundAlternating { get; init; }
    internal bool? BackgroundBorder { get; init; }
    internal string? BackgroundBorderColor { get; init; }
    internal string? GridSize { get; init; }
    internal string? GridOddColor { get; init; }
    internal string? GridEvenColor { get; init; }
    internal string? GridStroke { get; init; }
    internal string? GridOffsetX { get; init; }
    internal string? GridOffsetY { get; init; }
    internal bool? GridCoordinates { get; init; }
    internal string? CircleSize { get; init; }
    internal string? CircleColor { get; init; }
    internal string? CircleStroke { get; init; }
    internal string? CrosshairLength { get; init; }
    internal string? CrosshairColor { get; init; }
    internal string? CrosshairStroke { get; init; }
    internal string? LogoSource { get; init; }
    internal string? LogoX { get; init; }
    internal string? LogoY { get; init; }
    internal string? LogoWidth { get; init; }
    internal string? LogoHeight { get; init; }
    internal string? LogoOpacity { get; init; }
    internal bool? RenderDryRun { get; init; }
    internal bool? RenderNoDiscovery { get; init; }
    internal bool? RenderOutputsSkipUnspecified { get; init; }
    internal string? RenderOutput { get; init; }
    internal string? RenderVerbosity { get; init; }
    internal bool? RenderContinueAfterUnchanged { get; init; }
}
