// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Models;

internal record RenderOptions
{
    internal bool DryRun { get; init; }
    internal bool NoDiscovery { get; init; }
    internal bool OutputsSkipUnspecified { get; init; }
    internal string Output { get; init; } = RenderDefaults.Output;
    internal LogLevel MinimumLogLevel { get; init; } = LogLevel.Information;
    internal bool ContinueAfterUnchanged { get; init; }
    internal string MachineName { get; init; } = RenderDefaults.MachineName;
    internal bool SimulateNetwork { get; init; } = RenderDefaults.SimulateNetwork;
}