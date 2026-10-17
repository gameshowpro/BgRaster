// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Rendering;

internal interface ILayer
{
    void Render(RenderContext context, SKCanvas canvas);
}
