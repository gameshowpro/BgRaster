// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

public sealed class MaintenanceFeeNoticeTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "BgRasterTests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public void TryShowOnce_NoMarkerInteractive_WritesTextAndCreatesMarker()
    {
        string marker = Path.Combine(_root, "sub", "maintenance-fee-notice");
        StringWriter writer = new();

        bool shown = MaintenanceFeeNotice.TryShowOnce(writer, marker, interactive: true);

        Assert.True(shown);
        Assert.Contains("Open Source Maintenance Fee", writer.ToString());
        Assert.Contains("https://bgraster.gameshow.pro/maintenance-fee/", writer.ToString());
        Assert.True(File.Exists(marker));
    }

    [Fact]
    public void TryShowOnce_SecondCall_WritesNothing()
    {
        string marker = Path.Combine(_root, "maintenance-fee-notice");
        MaintenanceFeeNotice.TryShowOnce(new StringWriter(), marker, interactive: true);
        StringWriter second = new();

        bool shown = MaintenanceFeeNotice.TryShowOnce(second, marker, interactive: true);

        Assert.False(shown);
        Assert.Equal(string.Empty, second.ToString());
    }

    [Fact]
    public void TryShowOnce_NonInteractive_WritesNothingAndCreatesNoMarker()
    {
        string marker = Path.Combine(_root, "maintenance-fee-notice");
        StringWriter writer = new();

        bool shown = MaintenanceFeeNotice.TryShowOnce(writer, marker, interactive: false);

        Assert.False(shown);
        Assert.Equal(string.Empty, writer.ToString());
        Assert.False(File.Exists(marker));
    }

    [Fact]
    public void TryShowOnce_PreexistingMarker_Suppresses()
    {
        Directory.CreateDirectory(_root);
        string marker = Path.Combine(_root, "maintenance-fee-notice");
        File.WriteAllText(marker, "x");
        StringWriter writer = new();

        Assert.False(MaintenanceFeeNotice.TryShowOnce(writer, marker, interactive: true));
        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void TryShowOnce_UnwritableMarkerLocation_DoesNotThrow()
    {
        // A file where the marker's parent directory should be makes directory creation fail.
        Directory.CreateDirectory(_root);
        string blocker = Path.Combine(_root, "blocker");
        File.WriteAllText(blocker, "x");
        string marker = Path.Combine(blocker, "maintenance-fee-notice");
        StringWriter writer = new();

        bool shown = MaintenanceFeeNotice.TryShowOnce(writer, marker, interactive: true);

        Assert.True(shown);
        Assert.False(File.Exists(marker));
    }

    [Fact]
    public void TryShowOnce_InvalidMarkerPath_DoesNotThrow()
    {
        StringWriter writer = new();

        Exception? ex = Record.Exception(() =>
            MaintenanceFeeNotice.TryShowOnce(writer, "bad\0path", interactive: true));

        Assert.Null(ex);
    }

    [Fact]
    public void TextAndFooter_AreAscii()
    {
        Assert.All(MaintenanceFeeNotice.Text + MaintenanceFeeNotice.Footer, c => Assert.True(c < 128));
    }
}
