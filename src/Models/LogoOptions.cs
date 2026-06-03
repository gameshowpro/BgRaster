namespace GameshowPro.BgRaster.Models;

record LogoOptions
{
    internal ImmutableArray<string> Source { get; init; } = ["pack://application:,,,/GameshowPro.BgRaster;component/resources/gsp.svg"];
    internal ImmutableArray<string> X { get; init; } = ["85vw"];
    internal ImmutableArray<string> Y { get; init; } = ["15vh"];
    internal ImmutableArray<string> Width { get; init; } = ["20vw"];
    internal ImmutableArray<string> Height { get; init; } = ["20vh"];
    internal ImmutableArray<float> Opacity { get; init; } = [1f];
}
