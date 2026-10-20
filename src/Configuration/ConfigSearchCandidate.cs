// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Configuration;

/// <summary>A default location searched for <c>config.toml</c> when no <c>--config</c> is given.</summary>
/// <param name="Path">The full path of the candidate config file.</param>
/// <param name="RequiresAdministratorControl">
/// True for the machine-wide location, which is used only if administrators control it
/// (see <see cref="ConfigLocationTrust"/>), because other accounts on the computer could otherwise write it.
/// </param>
internal readonly record struct ConfigSearchCandidate(string Path, bool RequiresAdministratorControl);
