// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Tests;

public class TextLayerLayoutTests
{
    [Fact]
    public void ComputeBaselineAdvance_UsesArithmeticMeanWhenCollisionIsLower()
    {
        float result = TextLayer.ComputeBaselineAdvance(
            topSizePx: 40f,
            bottomSizePx: 10f,
            topDescentPx: 3f,
            bottomAscentPx: -8f,
            lineHeightRatio: 1.2f,
            collisionGapPx: 1f);

        _ = result.Should().BeApproximately(30f, 0.001f);
    }

    [Fact]
    public void ComputeBaselineAdvance_UsesCollisionFloorWhenItExceedsOpticalDistance()
    {
        float result = TextLayer.ComputeBaselineAdvance(
            topSizePx: 12f,
            bottomSizePx: 12f,
            topDescentPx: 10f,
            bottomAscentPx: -8f,
            lineHeightRatio: 1.2f,
            collisionGapPx: 1f);

        _ = result.Should().BeApproximately(19f, 0.001f);
    }
}
