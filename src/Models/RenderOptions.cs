namespace GameshowPro.BgRaster.Models;

record RenderOptions
{
    internal bool DryRun { get; init; }
    internal bool OutputsSkipUnspecified { get; init; }
    internal string Output { get; init; } = "";
    internal LogLevel MinimumLogLevel { get; init; } = LogLevel.Information;
    internal bool ContinueAfterUnchanged { get; init; }
}
