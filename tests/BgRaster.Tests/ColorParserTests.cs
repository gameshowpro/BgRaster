// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

public class ColorParserTests
{
    [Fact]
    public void Parse_HexRgb_ReturnsOpaqueColor()
    {
        SKColor c = ColorParser.Parse("#FF0000");
        c.Red.Should().Be(255);
        c.Green.Should().Be(0);
        c.Blue.Should().Be(0);
        c.Alpha.Should().Be(255);
    }

    [Fact]
    public void Parse_HexRgba_AppliesAlpha()
    {
        SKColor c = ColorParser.Parse("#FF000080");
        c.Red.Should().Be(255);
        c.Alpha.Should().Be(128);
    }

    [Fact]
    public void Parse_HexWhite_ReturnsWhite()
    {
        SKColor c = ColorParser.Parse("#FFFFFF");
        c.Should().Be(SKColors.White);
    }

    [Fact]
    public void Parse_HexRgbShorthand_ReturnsOpaqueColor()
    {
        SKColor c = ColorParser.Parse("#0f8");
        c.Red.Should().Be(0x00);
        c.Green.Should().Be(0xff);
        c.Blue.Should().Be(0x88);
        c.Alpha.Should().Be(255);
    }

    [Fact]
    public void Parse_HexRgbaShorthand_AppliesAlpha()
    {
        SKColor c = ColorParser.Parse("#0f8c");
        c.Red.Should().Be(0x00);
        c.Green.Should().Be(0xff);
        c.Blue.Should().Be(0x88);
        c.Alpha.Should().Be(0xcc);
    }

    [Fact]
    public void Parse_Transparent_ReturnsTransparent()
    {
        SKColor c = ColorParser.Parse("transparent");
        c.Should().Be(SKColors.Transparent);
    }

    [Fact]
    public void Parse_TransparentCaseInsensitive()
    {
        SKColor c = ColorParser.Parse("TRANSPARENT");
        c.Should().Be(SKColors.Transparent);
    }

    [Fact]
    public void Parse_Rgb_ReturnsCorrectColor()
    {
        SKColor c = ColorParser.Parse("rgb(0, 128, 255)");
        c.Red.Should().Be(0);
        c.Green.Should().Be(128);
        c.Blue.Should().Be(255);
        c.Alpha.Should().Be(255);
    }

    [Fact]
    public void Parse_Rgba_AppliesAlpha()
    {
        SKColor c = ColorParser.Parse("rgba(0, 128, 255, 0.5)");
        c.Red.Should().Be(0);
        c.Green.Should().Be(128);
        c.Blue.Should().Be(255);
        c.Alpha.Should().BeInRange(127, 128);
    }

    [Fact]
    public void Parse_Hsl_PureGreen()
    {
        SKColor c = ColorParser.Parse("hsl(120, 100%, 50%)");
        c.Red.Should().Be(0);
        c.Green.Should().Be(255);
        c.Blue.Should().Be(0);
        c.Alpha.Should().Be(255);
    }

    [Fact]
    public void Parse_Hsla_AppliesAlpha()
    {
        SKColor c = ColorParser.Parse("hsla(120, 100%, 50%, 0.5)");
        c.Green.Should().Be(255);
        c.Alpha.Should().BeInRange(127, 128);
    }

    [Fact]
    public void Parse_InvalidInput_ThrowsFormatException()
    {
        Action act = () => ColorParser.Parse("notacolor");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TryParse_InvalidInput_ReturnsFalse()
    {
        bool result = ColorParser.TryParse("invalid", out SKColor _);
        result.Should().BeFalse();
    }
}
