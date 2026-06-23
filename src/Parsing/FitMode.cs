// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Parsing;

enum FitMode
{
    CropTL,
    CropTR,
    CropC,
    CropBL,
    CropBR,
    BestFit,
    CropToFill,
}

static class FitModeParser
{
    internal static FitMode Parse(string input) =>
        input.Trim() switch
        {
            string s when s.Equals("CropTL", StringComparison.OrdinalIgnoreCase) => FitMode.CropTL,
            string s when s.Equals("CropTR", StringComparison.OrdinalIgnoreCase) => FitMode.CropTR,
            string s when s.Equals("CropC", StringComparison.OrdinalIgnoreCase) => FitMode.CropC,
            string s when s.Equals("CropBL", StringComparison.OrdinalIgnoreCase) => FitMode.CropBL,
            string s when s.Equals("CropBR", StringComparison.OrdinalIgnoreCase) => FitMode.CropBR,
            string s when s.Equals("BestFit", StringComparison.OrdinalIgnoreCase) => FitMode.BestFit,
            string s when s.Equals("CropToFill", StringComparison.OrdinalIgnoreCase) => FitMode.CropToFill,
            _ => throw new FormatException($"Unknown fit mode '{input}'."),
        };
}
