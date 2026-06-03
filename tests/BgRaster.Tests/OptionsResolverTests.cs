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
    public void Resolve_FieldSubstitution_OutputWidthHeight()
    {
        GlobalOptions global = new()
        {
            Text = new TextOptions { Text = ["${OutputWidth}x${OutputHeight}"] },
        };

        string line = OptionsResolver.Resolve(global, MakeOutput(0, 1920, 1080), null).TextLines[0];
        line.Should().Be("1920x1080");
    }

    [Fact]
    public void Resolve_FieldSubstitution_OutputIndexTokens()
    {
        GlobalOptions global = new()
        {
            Text = new TextOptions { Text = ["${OutputIndex}|${OutputIndexPlusOne}|${OutputLetter}"] },
        };

        string line = OptionsResolver.Resolve(global, MakeOutput(27), null).TextLines[0];
        line.Should().Be("27|28|AB");
    }

    [Fact]
    public void Resolve_FieldSubstitution_OutputLetterMinusOneToken()
    {
        GlobalOptions global = new()
        {
            Text = new TextOptions { Text = ["${OutputLetterMinusOne}"] },
        };

        string zero = OptionsResolver.Resolve(global, MakeOutput(0), null).TextLines[0];
        string one = OptionsResolver.Resolve(global, MakeOutput(1), null).TextLines[0];
        string twentySeven = OptionsResolver.Resolve(global, MakeOutput(27), null).TextLines[0];

        zero.Should().Be("#");
        one.Should().Be("A");
        twentySeven.Should().Be("AA");
    }

    [Fact]
    public void ResolveForSlice_FieldSubstitution_SliceTokens()
    {
        GlobalOptions global = new()
        {
            Text = new TextOptions { Text = ["${SliceIndex}|${SliceIndexPlusOne}|${SliceLetter}|${SliceWidth}x${SliceHeight}"] },
        };
        SliceOptions slice = new()
        {
            X = "0",
            Y = "0",
            Width = "100px",
            Height = "100px",
        };

        string line = OptionsResolver.ResolveForSlice(global, MakeOutput(1), null, slice, 100, 100, sliceIndex: 1).TextLines[0];
        line.Should().Be("1|2|B|100x100");
    }

    [Fact]
    public void ResolveForSlice_FieldSubstitution_SliceLetterMinusOneToken()
    {
        GlobalOptions global = new()
        {
            Text = new TextOptions { Text = ["${SliceLetterMinusOne}"] },
        };
        SliceOptions slice = new()
        {
            X = "0",
            Y = "0",
            Width = "100px",
            Height = "100px",
        };

        string zero = OptionsResolver.ResolveForSlice(global, MakeOutput(1), null, slice, 100, 100, sliceIndex: 0).TextLines[0];
        string one = OptionsResolver.ResolveForSlice(global, MakeOutput(1), null, slice, 100, 100, sliceIndex: 1).TextLines[0];
        string twentySeven = OptionsResolver.ResolveForSlice(global, MakeOutput(1), null, slice, 100, 100, sliceIndex: 27).TextLines[0];

        zero.Should().Be("#");
        one.Should().Be("A");
        twentySeven.Should().Be("AA");
    }

    [Fact]
    public void Resolve_FieldSubstitution_AppliesToBackgroundImageAndLogoSource()
    {
        GlobalOptions global = new()
        {
            Background = new BackgroundOptions { Image = ["assets/${MachineName}/${OutputName}.png"] },
            Logo = new LogoOptions { Source = ["assets/${OutputIndexPlusOne}/${SliceWidth}x${SliceHeight}.svg"] },
        };

        ResolvedOptions resolved = OptionsResolver.Resolve(global, MakeOutput(2, 1920, 1080), null);

        resolved.BackgroundImage.Should().Contain(Environment.MachineName);
        resolved.BackgroundImage.Should().Contain("Display 2");
        resolved.LogoSource.Should().Contain("3");
        resolved.LogoSource.Should().Contain("1920x1080");
    }

    [Fact]
    public void Resolve_CliRelativePaths_UseCurrentWorkingDirectory_WhenNoConfigIsLoaded()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        GlobalOptions options = ConfigLoader.ApplyCliOverlay(
            new GlobalOptions(),
            new CliOverlay
            {
                BackgroundImage = "assets/bg.png",
                LogoSource = "assets/logo.svg",
            });

        ResolvedOptions resolved = OptionsResolver.Resolve(options, MakeOutput(0), null);

        resolved.BackgroundImage.Should().Be(Path.GetFullPath(Path.Combine(currentDirectory, "assets", "bg.png")));
        resolved.LogoSource.Should().Be(Path.GetFullPath(Path.Combine(currentDirectory, "assets", "logo.svg")));
    }

    [Fact]
    public void Resolve_TextPrecedence_IsSliceThenOutputThenCliThenFileThenDefault()
    {
        OutputRecord output = MakeOutput(0);
        OutputOptions outputConfig = new()
        {
            Target = OutputTarget.FromIndex(0),
            Text = new TextOverride { Text = ["output"] },
        };
        SliceOptions sliceWithText = new()
        {
            X = "0",
            Y = "0",
            Width = "100px",
            Height = "100px",
            Text = new TextOverride { Text = ["slice"] },
        };
        SliceOptions sliceWithoutText = new()
        {
            X = "0",
            Y = "0",
            Width = "100px",
            Height = "100px",
        };

        GlobalOptions fileGlobal = new() { Text = new TextOptions { Text = ["file"] } };
        GlobalOptions cliGlobal = ConfigLoader.ApplyCliOverlay(fileGlobal, new CliOverlay { Text = "cli" });

        OptionsResolver.ResolveForSlice(cliGlobal, output, outputConfig, sliceWithText, 100, 100)
            .TextLines[0]
            .Should().Be("slice");
        OptionsResolver.Resolve(cliGlobal, output, outputConfig)
            .TextLines[0]
            .Should().Be("output");

        OutputOptions noOutputText = outputConfig with { Text = null };
        OptionsResolver.Resolve(cliGlobal, output, noOutputText)
            .TextLines[0]
            .Should().Be("cli");
        OptionsResolver.ResolveForSlice(cliGlobal, output, noOutputText, sliceWithoutText, 100, 100)
            .TextLines[0]
            .Should().Be("cli");

        GlobalOptions fileOnlyGlobal = fileGlobal;
        OptionsResolver.Resolve(fileOnlyGlobal, output, noOutputText)
            .TextLines[0]
            .Should().Be("file");

        GlobalOptions defaultGlobal = new() { Text = new TextOptions() };
        OptionsResolver.Resolve(defaultGlobal, output, null)
            .TextLines[0]
            .Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ResolveForSlice_GlobalArrays_CycleBySliceSequenceIndex()
    {
        GlobalOptions global = new()
        {
            Background = new BackgroundOptions { Color = ["#FF0000", "#00FF00", "#0000FF"] },
        };
        OutputRecord output = MakeOutput(0);
        SliceOptions slice = new()
        {
            X = "0",
            Y = "0",
            Width = "100px",
            Height = "100px",
        };

        SKColor first = OptionsResolver.ResolveForSlice(global, output, null, slice, 100, 100, sequenceIndex: 0).BackgroundColor;
        SKColor second = OptionsResolver.ResolveForSlice(global, output, null, slice, 100, 100, sequenceIndex: 1).BackgroundColor;
        SKColor third = OptionsResolver.ResolveForSlice(global, output, null, slice, 100, 100, sequenceIndex: 2).BackgroundColor;

        first.Should().Be(new SKColor(255, 0, 0));
        second.Should().Be(new SKColor(0, 255, 0));
        third.Should().Be(new SKColor(0, 0, 255));
    }

    [Fact]
    public void ResolveForSlice_DefaultText_UsesSliceScopedTemplate()
    {
        GlobalOptions global = new();
        SliceOptions slice = new()
        {
            X = "0",
            Y = "0",
            Width = "100px",
            Height = "50px",
        };

        ResolvedOptions resolved = OptionsResolver.ResolveForSlice(
            global,
            MakeOutput(0),
            null,
            slice,
            100,
            50,
            sliceIndex: 1,
            isImplicitSlice: false);

        resolved.TextLines[0].Should().Contain("output 1");
        resolved.TextLines[1].Should().Be("slice B");
        resolved.TextLines[2].Should().Be("100x50");
    }

    [Fact]
    public void ResolveForSlice_ImplicitSlice_DefaultsToOutputTemplate()
    {
        GlobalOptions global = new();
        SliceOptions implicitSlice = new()
        {
            X = "0",
            Y = "0",
            Width = "100vw",
            Height = "100vh",
        };

        ResolvedOptions resolved = OptionsResolver.ResolveForSlice(
            global,
            MakeOutput(2, 1920, 1080),
            null,
            implicitSlice,
            1920,
            1080,
            sliceIndex: 0,
            isImplicitSlice: true);

        resolved.TextLines[0].Should().Contain("2");
        resolved.TextLines[1].Should().Be("Display 2");
        resolved.TextLines[2].Should().Be("1920x1080");
    }

    [Fact]
    public void ResolveForSlice_ImplicitSlice_RespectsConfiguredGlobalText()
    {
        GlobalOptions global = new()
        {
            Text = new TextOptions { Text = ["configured"] },
        };
        SliceOptions implicitSlice = new()
        {
            X = "0",
            Y = "0",
            Width = "100vw",
            Height = "100vh",
        };

        ResolvedOptions resolved = OptionsResolver.ResolveForSlice(
            global,
            MakeOutput(0),
            null,
            implicitSlice,
            100,
            100,
            isImplicitSlice: true);

        resolved.TextLines[0].Should().Be("configured");
    }

    [Fact]
    public void ResolveForSlice_GridSize_UsesSliceOverrideValue()
    {
        GlobalOptions global = new()
        {
            Grid = new GridOptions { Size = ["100px"] },
        };
        OutputOptions outputConfig = new()
        {
            Target = OutputTarget.FromIndex(0),
        };
        SliceOptions slice = new()
        {
            X = "0",
            Y = "0",
            Width = "200px",
            Height = "400px",
            Grid = new GridOverride { Size = "25vh" },
        };

        ResolvedOptions resolved = OptionsResolver.ResolveForSlice(global, MakeOutput(0), outputConfig, slice, 200, 400);

        resolved.GridSizePx.Should().BeApproximately(100f, 0.01f);
    }

    [Fact]
    public void ResolveForSlice_LabeledEdges_UsesSliceOverrideAndParsesSides()
    {
        GlobalOptions global = new()
        {
            LabeledEdges = new LabeledEdgesOptions
            {
                TextSize = ["10px"],
                TailLength = ["4px"],
                Thickness = ["2px"],
                HeadScale = [1f],
                Scope = [LabeledEdgesScope.Output],
                Side = [LabeledEdgeSide.TL],
            },
        };
        OutputOptions outputConfig = new()
        {
            Target = OutputTarget.FromIndex(0),
            LabeledEdges = new LabeledEdgesOverride
            {
                TextSize = "12px",
                TailLength = "6px",
                Thickness = "3px",
                HeadScale = 1.5f,
                Scope = "Slice",
                Side = [LabeledEdgeSide.BR, LabeledEdgeSide.T],
            },
        };
        SliceOptions slice = new()
        {
            X = "0",
            Y = "0",
            Width = "200px",
            Height = "100px",
            LabeledEdges = new LabeledEdgesOverride
            {
                TextSize = "5vh",
                TailLength = "8px",
                Thickness = "4px",
                HeadScale = 2f,
            },
        };

        ResolvedOptions resolved = OptionsResolver.ResolveForSlice(global, MakeOutput(0), outputConfig, slice, 200, 100);

        resolved.LabeledEdgesTextSizePx.Should().BeApproximately(5f, 0.01f);
        resolved.LabeledEdgesTailLengthPx.Should().BeApproximately(8f, 0.01f);
        resolved.LabeledEdgesThicknessPx.Should().BeApproximately(4f, 0.01f);
        resolved.LabeledEdgesHeadScale.Should().BeApproximately(2f, 0.01f);
        resolved.LabeledEdgesScope.Should().Be(LabeledEdgesScope.Slice);
        resolved.LabeledEdgesSides.Should().Equal([LabeledEdgeSide.BR, LabeledEdgeSide.T]);
    }
}
