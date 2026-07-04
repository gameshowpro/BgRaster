// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

using Microsoft.Extensions.Logging;

namespace GameshowPro.BgRaster.Models;

record RenderOptions
{
    internal bool DryRun { get; init; }
    internal bool NoDiscovery { get; init; }
    internal bool OutputsSkipUnspecified { get; init; }
    internal string Output { get; init; } = Configuration.RenderDefaults.Output;
    internal LogLevel MinimumLogLevel { get; init; } = LogLevel.Information;
    internal bool ContinueAfterUnchanged { get; init; }
    internal string MachineName { get; init; } = Configuration.RenderDefaults.MachineName;
    internal bool SimulateNetwork { get; init; } = Configuration.RenderDefaults.SimulateNetwork;
}