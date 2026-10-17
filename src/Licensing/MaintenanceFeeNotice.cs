// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster;

/// <summary>
/// Open Source Maintenance Fee messaging. Education only: nothing is checked or sent.
/// </summary>
internal static class MaintenanceFeeNotice
{
    internal const string Text =
        "BgRaster participates in the Open Source Maintenance Fee. The source is free\n" +
        "under MPL-2.0. Organizations that use official BgRaster builds as part of\n" +
        "revenue-generating activities pay a small monthly fee that funds maintenance.\n" +
        "Who pays and how: https://bgraster.gameshow.pro/maintenance-fee/\n" +
        "Shown once per Windows user. Nothing is checked or sent.";

    internal const string Footer =
        "Open source under MPL-2.0. Official builds participate in the Open Source Maintenance Fee: https://bgraster.gameshow.pro/maintenance-fee/";

    internal static string GetMarkerPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BgRaster",
            "maintenance-fee-notice");
    }

    /// <summary>
    /// Writes the notice once per marker. Never throws.
    /// </summary>
    /// <returns>True if the notice was written.</returns>
    internal static bool TryShowOnce(TextWriter writer, string markerPath, bool interactive)
    {
        try
        {
            if (!interactive || File.Exists(markerPath))
            {
                return false;
            }

            writer.WriteLine();
            writer.WriteLine(Text);
            writer.Flush();
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            string? directory = Path.GetDirectoryName(markerPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "0.0.0";
            File.WriteAllText(markerPath, $"{DateTime.UtcNow:O} {version}{Environment.NewLine}");
        }
        catch (Exception)
        {
            // The notice may show again next run, which is acceptable.
        }

        return true;
    }

    /// <summary>
    /// Shows the notice to the console error stream if appropriate. Never throws.
    /// </summary>
    internal static void ShowIfDue()
    {
        try
        {
            bool interactive = !Console.IsOutputRedirected && !Console.IsErrorRedirected;
            TryShowOnce(Console.Error, GetMarkerPath(), interactive);
        }
        catch (Exception)
        {
        }
    }
}
