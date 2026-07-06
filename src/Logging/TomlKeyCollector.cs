// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Logging;

internal static class TomlKeyCollector
{
    internal static HashSet<string> Collect(TomlTable root)
    {
        HashSet<string> paths = new(StringComparer.Ordinal);
        WalkTable(root, "", paths);
        return paths;
    }

    private static void WalkTable(TomlTable table, string prefix, HashSet<string> paths)
    {
        foreach (KeyValuePair<string, object> kv in table)
        {
            string key = kv.Key;
            string path = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";

            switch (kv.Value)
            {
                case TomlTable childTable:
                    WalkTable(childTable, path, paths);
                    break;

                case TomlTableArray arr:
                    for (int i = 0; i < arr.Count; i++)
                    {
                        string arrayPath = $"{path}[{i}]";
                        WalkTable(arr[i], arrayPath, paths);
                    }
                    break;

                default:
                    _ = paths.Add(path);
                    break;
            }
        }
    }
}
