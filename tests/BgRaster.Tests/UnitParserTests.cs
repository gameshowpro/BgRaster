// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

public class UnitParserTests
{
    [Theory]
    [InlineData("100px", 100f, "Px")]
    [InlineData("50vw", 50f, "Vw")]
    [InlineData("25vh", 25f, "Vh")]
    [InlineData("5vmin", 5f, "Vmin")]
    [InlineData("10vmax", 10f, "Vmax")]
    [InlineData("0", 0f, "Px")]
    [InlineData("1.5vh", 1.5f, "Vh")]
    [InlineData("100VW", 100f, "Vw")]
    [InlineData(" 50 vw ", 50f, "Vw")]
    [InlineData("200", 200f, "Px")]
    public void Parse_ValidInput_ReturnsExpected(string input, float expectedValue, string expectedUnitName)
    {
        DimensionUnit expectedUnit = Enum.Parse<DimensionUnit>(expectedUnitName);
        UnitValue result = UnitParser.Parse(input);
        _ = result.Value.Should().BeApproximately(expectedValue, 0.001f);
        _ = result.Unit.Should().Be(expectedUnit);
    }

    [Fact]
    public void Parse_Invalid_ThrowsFormatException()
    {
        Action act = () => UnitParser.Parse("abc");
        _ = act.Should().Throw<FormatException>();
    }

    [Fact]
    public void ResolvePixels_Vh_UsesViewportHeight()
    {
        UnitValue uv = UnitParser.Parse("50vh");
        float px = uv.ResolvePixels(1920f, 1080f);
        _ = px.Should().BeApproximately(540f, 0.1f);
    }

    [Fact]
    public void ResolvePixels_Vw_UsesViewportWidth()
    {
        UnitValue uv = UnitParser.Parse("50vw");
        float px = uv.ResolvePixels(1920f, 1080f);
        _ = px.Should().BeApproximately(960f, 0.1f);
    }

    [Fact]
    public void ResolvePixels_Vmin_UsesMinDimension()
    {
        UnitValue uv = UnitParser.Parse("5vmin");
        float px = uv.ResolvePixels(1920f, 1080f);
        _ = px.Should().BeApproximately(54f, 0.1f);
    }

    [Fact]
    public void ResolvePixels_Vmax_UsesMaxDimension()
    {
        UnitValue uv = UnitParser.Parse("10vmax");
        float px = uv.ResolvePixels(1920f, 1080f);
        _ = px.Should().BeApproximately(192f, 0.1f);
    }

    [Fact]
    public void ResolvePixels_Px_Passthrough()
    {
        UnitValue uv = UnitParser.Parse("128px");
        float px = uv.ResolvePixels(1920f, 1080f);
        _ = px.Should().BeApproximately(128f, 0.001f);
    }
}
