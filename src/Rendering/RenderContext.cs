// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering;

record RenderContext(
    OutputRecord OutputRecord,
    ResolvedOptions Options,
    int ViewportWidth,
    int ViewportHeight,
    int CanvasOffsetX,
    int CanvasOffsetY);
