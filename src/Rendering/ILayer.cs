// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering;

interface ILayer
{
    void Render(RenderContext context, SKCanvas canvas);
}
