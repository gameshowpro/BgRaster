// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.StateCache;

internal static class LastRunReader
{
    internal static LastRunState? Read(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            string toml = File.ReadAllText(path);
            TomlTable table = Toml.ToModel(toml);
            return ParseLastRunState(table);
        }
        catch (Exception ex)
        {
            TryDeleteUnreadableFile(path);
            Console.WriteLine($"LastRunReader: could not read '{path}': {ex.Message} - will regenerate.");
            return null;
        }
    }

    private static void TryDeleteUnreadableFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Best effort only: keep read failures non-fatal.
        }
    }

    private static LastRunState ParseLastRunState(TomlTable table)
    {
        LastRunMeta meta = table.TryGetValue("meta", out object? metaObj) && metaObj is TomlTable metaTable
            ? ParseMeta(metaTable)
            : new LastRunMeta();

        ImmutableArray<OutputRecord> hardware = table.TryGetValue("hardware_output", out object? hwObj) && hwObj is TomlTableArray hwArray
            ? [.. hwArray.Select(ParseOutputRecord)]
            : [];

        return new LastRunState
        {
            Meta = meta,
            HardwareOutputs = hardware,
        };
    }

    private static LastRunMeta ParseMeta(TomlTable t)
    {
        Dictionary<string, string> assignedFiles = [];
        if (t.TryGetValue("assignedFiles", out object? afObj) && afObj is TomlTable afTable)
        {
            foreach (KeyValuePair<string, object> kv in afTable)
            {
                if (kv.Value is string v)
                {
                    assignedFiles[kv.Key] = v;
                }
            }
        }

        ImmutableArray<string> unrecycled = t.TryGetValue("unrecycledFiles", out object? urfObj) && urfObj is TomlArray urfArray
            ? [.. urfArray.OfType<string>()]
            : [];

        return new LastRunMeta
        {
            Version = GetString(t, "version") ?? "",
            SettingsHash = GetString(t, "settingsHash") ?? "",
            Timestamp = GetString(t, "timestamp") ?? "",
            AssignedFiles = assignedFiles.ToFrozenDictionary(),
            UnrecycledFiles = unrecycled,
        };
    }

    private static OutputRecord ParseOutputRecord(TomlTable t) => new()
    {
        Id = GetString(t, "id") ?? "",
        Index = GetInt(t, "index"),
        DesktopX = GetInt(t, "desktopX"),
        DesktopY = GetInt(t, "desktopY"),
        WidthPx = GetInt(t, "widthPx"),
        HeightPx = GetInt(t, "heightPx"),
        DpiX = GetInt(t, "dpiX"),
        DpiY = GetInt(t, "dpiY"),
        Rotation = GetInt(t, "rotation"),
        AdapterName = GetString(t, "adapterName") ?? "",
        FriendlyName = GetString(t, "friendlyName") ?? "",
    };

    private static string? GetString(TomlTable t, string key) =>
        t.TryGetValue(key, out object? v) && v is string s ? s : null;

    private static int GetInt(TomlTable t, string key) =>
        t.TryGetValue(key, out object? v) && v is long l ? (int)l : 0;
}
