namespace GameshowPro.BgRaster.Logging;

internal static partial class BgRasterLog
{
    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "# bg-raster: status=run-start configPath=\"{configPath}\" configExists={configExists} outputCount={outputCount} dryRun={dryRun} continueAfterUnchanged={continueAfterUnchanged} minLogLevel={minLogLevel}")]
    internal static partial void RunStart(this ILogger logger, string configPath, bool configExists, int outputCount, bool dryRun, bool continueAfterUnchanged, LogLevel minLogLevel);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "# bg-raster: status=io-paths outputDir=\"{outputDir}\" lastRunPath=\"{lastRunPath}\"")]
    internal static partial void IoPaths(this ILogger logger, string outputDir, string lastRunPath);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "# bg-raster: status=settings-hash value={settingsHash}")]
    internal static partial void SettingsHashComputed(this ILogger logger, string settingsHash);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "# bg-raster: status=last-run-load found={found} path=\"{path}\"")]
    internal static partial void LastRunLoad(this ILogger logger, bool found, string path);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "# bg-raster: status=hardware-discovered count={count}")]
    internal static partial void HardwareDiscovered(this ILogger logger, int count);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "# bg-raster: status=hardware-output id=\"{id}\" index={index} position={desktopX},{desktopY} resolution={widthPx}x{heightPx} rotation={rotation} dpi={dpiX}x{dpiY}")]
    internal static partial void HardwareOutput(this ILogger logger, string id, int index, int desktopX, int desktopY, int widthPx, int heightPx, int rotation, int dpiX, int dpiY);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "# bg-raster: status=matching-start configuredOutputs={configuredOutputs}")]
    internal static partial void MatchingStart(this ILogger logger, int configuredOutputs);

    [LoggerMessage(EventId = 9, Level = LogLevel.Debug, Message = "# bg-raster: status=matching-finished results={results} matchedOutputs={matchedOutputs}")]
    internal static partial void MatchingFinished(this ILogger logger, int results, int matchedOutputs);

    [LoggerMessage(EventId = 18, Level = LogLevel.Warning, Message = "# bg-raster: status=no-configured-outputs reason=effective-config-has-zero-output-blocks")]
    internal static partial void NoConfiguredOutputs(this ILogger logger);

    [LoggerMessage(EventId = 19, Level = LogLevel.Warning, Message = "# bg-raster: status=no-rendered-files reason=zero-matched-outputs")]
    internal static partial void NoRenderedFiles(this ILogger logger);

    [LoggerMessage(EventId = 13, Level = LogLevel.Debug, Message = "# bg-raster: status=render-start id=\"{outputId}\" target={target}")]
    internal static partial void RenderStart(this ILogger logger, string outputId, string target);

    [LoggerMessage(EventId = 14, Level = LogLevel.Debug, Message = "# bg-raster: status=assign-start fileCount={fileCount}")]
    internal static partial void AssignStart(this ILogger logger, int fileCount);

    [LoggerMessage(EventId = 15, Level = LogLevel.Debug, Message = "# bg-raster: status=assign-result success={success} fileCount={fileCount}")]
    internal static partial void AssignResult(this ILogger logger, bool success, int fileCount);

    [LoggerMessage(EventId = 16, Level = LogLevel.Debug, Message = "# bg-raster: status=stale-scan staleCount={staleCount} recycledCount={recycledCount}")]
    internal static partial void StaleScan(this ILogger logger, int staleCount, int recycledCount);

    [LoggerMessage(EventId = 17, Level = LogLevel.Debug, Message = "# bg-raster: status=last-run-write path=\"{path}\" assignedCount={assignedCount} unrecycledCount={unrecycledCount}")]
    internal static partial void LastRunWrite(this ILogger logger, string path, int assignedCount, int unrecycledCount);

    // ── Configuration warnings ────────────────────────────────────────────────

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "{warning}")]
    internal static partial void ConfigurationWarning(this ILogger logger, string warning);

    // ── Effective-config dump ─────────────────────────────────────────────────

    [LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "# bg-raster: status=effective-config-begin")]
    internal static partial void EffectiveConfigBegin(this ILogger logger);

    [LoggerMessage(EventId = 11, Level = LogLevel.Debug, Message = "{line}")]
    internal static partial void EffectiveConfigLine(this ILogger logger, string line);

    [LoggerMessage(EventId = 12, Level = LogLevel.Debug, Message = "# bg-raster: status=effective-config-end")]
    internal static partial void EffectiveConfigEnd(this ILogger logger);

    // ── Run lifecycle ─────────────────────────────────────────────────────────

    [LoggerMessage(EventId = 20, Level = LogLevel.Information, Message = "# bg-raster: status=run-skipped-unchanged")]
    internal static partial void RunSkippedUnchanged(this ILogger logger);

    [LoggerMessage(EventId = 21, Level = LogLevel.Information, Message = "# bg-raster: status=output-rendered id=\"{outputId}\" file=\"{filePath}\"")]
    internal static partial void OutputRendered(this ILogger logger, string outputId, string filePath);

    [LoggerMessage(EventId = 22, Level = LogLevel.Information, Message = "# bg-raster: status=output-not-found target={target}")]
    internal static partial void OutputNotFound(this ILogger logger, string target);

    [LoggerMessage(EventId = 23, Level = LogLevel.Information, Message = "# bg-raster: status=duplicate-output-ignored target={target}")]
    internal static partial void DuplicateOutputIgnored(this ILogger logger, string target);

    [LoggerMessage(EventId = 24, Level = LogLevel.Warning, Message = "# bg-raster: status=wallpaper-assignment-failed")]
    internal static partial void WallpaperAssignmentFailed(this ILogger logger);

    [LoggerMessage(EventId = 25, Level = LogLevel.Information, Message = "# bg-raster: status=run-complete")]
    internal static partial void RunComplete(this ILogger logger);

    [LoggerMessage(EventId = 26, Level = LogLevel.Debug, Message = "# bg-raster: status=unchanged-check matchedLastRun={matchedLastRun} continueAfterUnchanged={continueAfterUnchanged}")]
    internal static partial void UnchangedCheck(this ILogger logger, bool matchedLastRun, bool continueAfterUnchanged);

    [LoggerMessage(EventId = 27, Level = LogLevel.Debug, Message = "# bg-raster: status=skip-because-unchanged")]
    internal static partial void SkipBecauseUnchanged(this ILogger logger);

    [LoggerMessage(EventId = 28, Level = LogLevel.Debug, Message = "# bg-raster: status=continue-after-unchanged reason=force")]
    internal static partial void ContinueAfterUnchanged(this ILogger logger);

    [LoggerMessage(EventId = 29, Level = LogLevel.Debug, Message = "# bg-raster: status=assignment-skipped reason=no-rendered-files")]
    internal static partial void AssignmentSkippedNoRenderedFiles(this ILogger logger);

    [LoggerMessage(EventId = 30, Level = LogLevel.Information, Message = "# bg-raster: status=execution-time elapsedMs={elapsedMs} elapsed={elapsed} exitCode={exitCode}")]
    internal static partial void ExecutionTime(this ILogger logger, long elapsedMs, string elapsed, int exitCode);

    // ── LastRunWriter diagnostics ─────────────────────────────────────────────

    [LoggerMessage(EventId = 40, Level = LogLevel.Warning, Message = "LastRunWriter: round-trip verification failed for '{path}'; previous file kept.")]
    internal static partial void RoundTripFailed(this ILogger logger, string path);

    [LoggerMessage(EventId = 41, Level = LogLevel.Warning, Message = "LastRunWriter: written file did not parse back.")]
    internal static partial void RoundTripDidNotParse(this ILogger logger);

    [LoggerMessage(EventId = 42, Level = LogLevel.Warning, Message = "LastRunWriter: meta scalar mismatch on round-trip.")]
    internal static partial void RoundTripMetaScalarMismatch(this ILogger logger);

    [LoggerMessage(EventId = 43, Level = LogLevel.Warning, Message = "LastRunWriter: meta.assignedFiles mismatch on round-trip.")]
    internal static partial void RoundTripAssignedFilesMismatch(this ILogger logger);

    [LoggerMessage(EventId = 44, Level = LogLevel.Warning, Message = "LastRunWriter: meta.unrecycledFiles mismatch on round-trip.")]
    internal static partial void RoundTripUnrecycledFilesMismatch(this ILogger logger);

    [LoggerMessage(EventId = 45, Level = LogLevel.Warning, Message = "LastRunWriter: hardware_output count mismatch on round-trip.")]
    internal static partial void RoundTripHardwareCountMismatch(this ILogger logger);

    [LoggerMessage(EventId = 46, Level = LogLevel.Warning, Message = "LastRunWriter: hardware_output[{index}] mismatch on round-trip.")]
    internal static partial void RoundTripHardwareItemMismatch(this ILogger logger, int index);

    [LoggerMessage(EventId = 47, Level = LogLevel.Error, Message = "LastRunWriter: failed to write '{path}'.")]
    internal static partial void WriteFailure(this ILogger logger, Exception exception, string path);
}
