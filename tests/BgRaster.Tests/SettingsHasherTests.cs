namespace GameshowPro.BgRaster.Tests;

public class SettingsHasherTests
{
    [Fact]
    public void Compute_SameOptions_ReturnsSameHash()
    {
        GlobalOptions options = new();
        string hash1 = SettingsHasher.Compute(options);
        string hash2 = SettingsHasher.Compute(options);
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Compute_DifferentBackgroundColor_ReturnsDifferentHash()
    {
        GlobalOptions a = new()
        {
            Background = new BackgroundOptions { Color = ["#FF0000"] },
        };
        GlobalOptions b = new()
        {
            Background = new BackgroundOptions { Color = ["#00FF00"] },
        };

        SettingsHasher.Compute(a).Should().NotBe(SettingsHasher.Compute(b));
    }

    [Fact]
    public void Compute_EmptyOptions_ReturnsStableHash()
    {
        string hash = SettingsHasher.Compute(new GlobalOptions());
        hash.Should().HaveLength(64); // SHA-256 hex = 32 bytes = 64 hex chars
        hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Compute_CliOverlayChangesHash()
    {
        GlobalOptions base_ = new();
        CliOverlay overlay = new() { BackgroundColor = "#000000" };
        GlobalOptions modified = ConfigLoader.ApplyCliOverlay(base_, overlay);

        SettingsHasher.Compute(base_).Should().NotBe(SettingsHasher.Compute(modified));
    }
}
