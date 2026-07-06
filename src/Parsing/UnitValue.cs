// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Parsing;

internal readonly record struct UnitValue(float Value, DimensionUnit Unit)
{
    internal float ResolvePixels(float viewportWidth, float viewportHeight) => Unit switch
    {
        DimensionUnit.Px => Value,
        DimensionUnit.Vw => Value / 100f * viewportWidth,
        DimensionUnit.Vh => Value / 100f * viewportHeight,
        DimensionUnit.Vmin => Value / 100f * MathF.Min(viewportWidth, viewportHeight),
        DimensionUnit.Vmax => Value / 100f * MathF.Max(viewportWidth, viewportHeight),
        _ => Value,
    };
}
