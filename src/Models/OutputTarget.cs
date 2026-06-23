// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

abstract record OutputTarget
{
    internal sealed record IndexTarget(int Index) : OutputTarget;
    internal sealed record IdTarget(string Id) : OutputTarget;

    internal static OutputTarget FromIndex(int index) => new IndexTarget(index);
    internal static OutputTarget FromId(string id) => new IdTarget(id);
}
