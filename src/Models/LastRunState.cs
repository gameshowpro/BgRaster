namespace GameshowPro.BgRaster.Models;

record LastRunState
{
    internal LastRunMeta Meta { get; init; } = new();
    internal ImmutableArray<OutputRecord> HardwareOutputs { get; init; } = [];
    internal GlobalOptions EffectiveConfig { get; init; } = new();
}
