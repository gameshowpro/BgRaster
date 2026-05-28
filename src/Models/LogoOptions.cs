namespace GameshowPro.BgRaster.Models;

record LogoOptions
{
    internal ImmutableArray<string> Source { get; init; } = [""];
    internal ImmutableArray<string> X { get; init; } = ["75vw"];
    internal ImmutableArray<string> Y { get; init; } = ["25vh"];
    internal ImmutableArray<string> Width { get; init; } = ["15vw"];
    internal ImmutableArray<string> Height { get; init; } = ["15vh"];
    internal ImmutableArray<float> Opacity { get; init; } = [1f];
}
