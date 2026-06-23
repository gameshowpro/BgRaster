// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.FileLifecycle;

class StaleFileCleaner
{
    internal ImmutableArray<string> FindStaleFiles(string directory, IReadOnlySet<string> currentRunFiles)
    {
        if (!Directory.Exists(directory))
            return [];

        return [.. Directory
            .EnumerateFiles(directory, "*.png")
            .Where(f => FileNamer.IsBgRasterFile(f) && !currentRunFiles.Contains(f))];
    }

    internal ImmutableArray<string> RecycleFiles(ImmutableArray<string> filePaths)
    {
        // TODO: implement IFileOperation Windows shell recycle bin
        // Until then, log intent and return all paths as unrecycled so they are retried next run.
        if (!filePaths.IsEmpty)
            Console.WriteLine($"StaleFileCleaner: {filePaths.Length} stale file(s) pending recycle (shell recycle not yet implemented).");
        return filePaths;
    }
}
