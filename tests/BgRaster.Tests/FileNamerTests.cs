namespace GameshowPro.BgRaster.Tests;

public class FileNamerTests
{
    [Fact]
    public void GenerateFileName_MatchesExpectedPattern()
    {
        string name = FileNamer.GenerateFileName("STUB\\DISPLAY#0");
        name.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}\.\d+Z_.+\.png$");
    }

    [Fact]
    public void GenerateFileName_SanitizesSpecialCharsInId()
    {
        string name = FileNamer.GenerateFileName(@"\\?\DISPLAY#SAM0C7F");
        name.Should().NotContain("\\");
        name.Should().NotContain("?");
        name.Should().NotContain("#");
    }

    [Fact]
    public void GenerateFileName_TruncatesLongIds()
    {
        string longId = new('A', 200);
        string name = FileNamer.GenerateFileName(longId);
        // The portion after timestamp_ should be at most 48 chars + ".png"
        int underscoreIdx = name.IndexOf('_');
        string idPart = name[(underscoreIdx + 1)..^4]; // strip timestamp_ and .png
        idPart.Length.Should().BeLessOrEqualTo(48);
    }

    [Fact]
    public void IsBgRasterFile_GeneratedName_ReturnsTrue()
    {
        string name = FileNamer.GenerateFileName("output0");
        FileNamer.IsBgRasterFile(name).Should().BeTrue();
    }

    [Fact]
    public void IsBgRasterFile_ArbitraryPng_ReturnsFalse()
    {
        FileNamer.IsBgRasterFile("wallpaper.png").Should().BeFalse();
    }

    [Fact]
    public void GetOutputDirectory_Empty_ContainsBgRaster()
    {
        string dir = FileNamer.GetOutputDirectory(null);
        dir.Should().Contain("BgRaster");
    }

    [Fact]
    public void GetOutputDirectory_Override_ReturnsOverride()
    {
        string dir = FileNamer.GetOutputDirectory("C:\\custom\\path");
        dir.Should().Be("C:\\custom\\path");
    }
}
