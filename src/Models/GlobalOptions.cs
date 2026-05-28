namespace GameshowPro.BgRaster.Models;

record GlobalOptions
{
    internal TextOptions Text { get; init; } = new();
    internal BackgroundOptions Background { get; init; } = new();
    internal GridOptions Grid { get; init; } = new();
    internal CircleOptions Circle { get; init; } = new();
    internal CrosshairOptions Crosshair { get; init; } = new();
    internal LogoOptions Logo { get; init; } = new();
    internal RenderOptions Render { get; init; } = new();
    internal ImmutableArray<OutputOptions> Outputs { get; init; } = [];
}
