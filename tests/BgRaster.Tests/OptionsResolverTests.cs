namespace GameshowPro.BgRaster.Tests;

public class OptionsResolverTests
{
    static OutputRecord MakeOutput(int index, int width = 1920, int height = 1080) => new()
    {
        Id = $"STUB\\DISPLAY#{index}",
        Index = index,
        WidthPx = width,
        HeightPx = height,
        FriendlyName = $"Display {index}",
    };

    [Fact]
    public void Resolve_GlobalArray_CyclesByOutputIndex()
    {
        GlobalOptions global = new()
        {
            Background = new BackgroundOptions { Color = ["#FF0000", "#00FF00", "#0000FF"] },
        };

        SKColor red = OptionsResolver.Resolve(global, MakeOutput(0), null).BackgroundColor;
        SKColor green = OptionsResolver.Resolve(global, MakeOutput(1), null).BackgroundColor;
        SKColor blue = OptionsResolver.Resolve(global, MakeOutput(2), null).BackgroundColor;

        red.Should().Be(new SKColor(255, 0, 0));
        green.Should().Be(new SKColor(0, 255, 0));
        blue.Should().Be(new SKColor(0, 0, 255));
    }

    [Fact]
    public void Resolve_GlobalArray_WrapsAround()
    {
        GlobalOptions global = new()
        {
            Background = new BackgroundOptions { Color = ["#FF0000", "#00FF00", "#0000FF"] },
        };

        SKColor color = OptionsResolver.Resolve(global, MakeOutput(3), null).BackgroundColor;
        color.Should().Be(new SKColor(255, 0, 0));
    }

    [Fact]
    public void Resolve_OutputOverride_TakesPrecedenceOverGlobal()
    {
        GlobalOptions global = new()
        {
            Background = new BackgroundOptions { Color = ["#FF0000"] },
        };
        OutputOptions outputConfig = new()
        {
            Target = OutputTarget.FromIndex(0),
            Background = new BackgroundOverride { Color = "#FFFFFF" },
        };

        SKColor color = OptionsResolver.Resolve(global, MakeOutput(0), outputConfig).BackgroundColor;
        color.Should().Be(SKColors.White);
    }

    [Fact]
    public void Resolve_SliceOverride_TakesPrecedenceOverOutputAndGlobal()
    {
        GlobalOptions global = new()
        {
            Background = new BackgroundOptions { Color = ["#FF0000"] },
        };
        OutputOptions outputConfig = new()
        {
            Target = OutputTarget.FromIndex(0),
            Background = new BackgroundOverride { Color = "#FFFFFF" },
        };
        SliceOptions slice = new()
        {
            X = "0", Y = "0", Width = "960px", Height = "1080px",
            Background = new BackgroundOverride { Color = "#000000" },
        };

        SKColor color = OptionsResolver.ResolveForSlice(global, MakeOutput(0), outputConfig, slice, 960, 1080).BackgroundColor;
        color.Should().Be(SKColors.Black);
    }

    [Fact]
    public void Resolve_SingleElementArray_SameForAllOutputs()
    {
        GlobalOptions global = new()
        {
            Background = new BackgroundOptions { Color = ["#AABBCC"] },
        };

        SKColor c0 = OptionsResolver.Resolve(global, MakeOutput(0), null).BackgroundColor;
        SKColor c5 = OptionsResolver.Resolve(global, MakeOutput(5), null).BackgroundColor;

        c0.Should().Be(c5);
    }

    [Fact]
    public void Resolve_FieldSubstitution_MachineName()
    {
        GlobalOptions global = new()
        {
            Text = new TextOptions { Text = ["Hello ${MachineName}"] },
        };

        string line = OptionsResolver.Resolve(global, MakeOutput(0), null).TextLines[0];
        line.Should().Contain(Environment.MachineName);
    }

    [Fact]
    public void Resolve_FieldSubstitution_WidthHeight()
    {
        GlobalOptions global = new()
        {
            Text = new TextOptions { Text = ["${Width}x${Height}"] },
        };

        string line = OptionsResolver.Resolve(global, MakeOutput(0, 1920, 1080), null).TextLines[0];
        line.Should().Be("1920x1080");
    }
}
