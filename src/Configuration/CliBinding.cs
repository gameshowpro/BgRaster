// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Configuration;

static class CliBinding
{
    internal static RootCommand BuildRootCommand(Func<string?, CliOverlay, Task<int>> handler)
    {
        Option<string?> configOption = CreateStringOption("--config");
        Option<string?> machineNameOption = CreateStringOption("--machine-name");
        Option<string?> textFormatOption = CreateStringOption("--text-format");
        Option<string?> textSizeOption = CreateStringOption("--text-size");
        Option<string?> textColorOption = CreateStringOption("--text-color");
        Option<string?> textXOption = CreateStringOption("--text-x");
        Option<string?> textYOption = CreateStringOption("--text-y");
        Option<string?> bgColorOption = CreateStringOption("--background-color");
        Option<string?> bgImageOption = CreateStringOption("--background-image");
        Option<string?> bgFitOption = CreateStringOption("--background-fit");
        Option<bool?> bgAlternatingOption = CreateNullableBoolOption("--background-alternating");
        Option<bool?> bgBorderOption = CreateNullableBoolOption("--background-border");
        Option<string?> bgBorderColorOption = CreateStringOption("--background-border-color");
        Option<string?> gridSizeOption = CreateStringOption("--grid-size");
        Option<string?> gridOddColorOption = CreateStringOption("--grid-odd-color");
        Option<string?> gridEvenColorOption = CreateStringOption("--grid-even-color");
        Option<string?> gridStrokeOption = CreateStringOption("--grid-stroke");
        Option<string?> gridOffsetXOption = CreateStringOption("--grid-offset-x");
        Option<string?> gridOffsetYOption = CreateStringOption("--grid-offset-y");
        Option<bool?> gridCoordinatesOption = CreateNullableBoolOption("--grid-coordinates");
        Option<string?> circleSizeOption = CreateStringOption("--circle-size");
        Option<string?> circleXOption = CreateStringOption("--circle-x");
        Option<string?> circleYOption = CreateStringOption("--circle-y");
        Option<string?> circleColorOption = CreateStringOption("--circle-color");
        Option<string?> circleStrokeOption = CreateStringOption("--circle-stroke");
        Option<string?> crosshairLengthOption = CreateStringOption("--crosshair-length");
        Option<string?> crosshairXOption = CreateStringOption("--crosshair-x");
        Option<string?> crosshairYOption = CreateStringOption("--crosshair-y");
        Option<string?> crosshairColorOption = CreateStringOption("--crosshair-color");
        Option<string?> crosshairStrokeOption = CreateStringOption("--crosshair-stroke");
        Option<string?> logoSourceOption = CreateStringOption("--logo-source");
        Option<string?> logoXOption = CreateStringOption("--logo-x");
        Option<string?> logoYOption = CreateStringOption("--logo-y");
        Option<string?> logoAnchorXOption = CreateStringOption("--logo-anchor-x");
        Option<string?> logoAnchorYOption = CreateStringOption("--logo-anchor-y");
        Option<string?> logoWidthOption = CreateStringOption("--logo-width");
        Option<string?> logoHeightOption = CreateStringOption("--logo-height");
        Option<string?> logoOpacityOption = CreateStringOption("--logo-opacity");
        Option<bool?> dryRunOption = CreateNullableBoolOption("--no-assignment");
        Option<bool?> noDiscoveryOption = CreateNullableBoolOption("--no-discovery");
        Option<bool?> outputsSkipUnspecifiedOption = CreateNullableBoolOption("--outputs-skip-unspecified");
        Option<string?> outputDirOption = CreateStringOption("--render-output");
        Option<bool?> continueAfterUnchangedOption = CreateNullableBoolOption("--render-force");
        Option<string?> verbosityOption = CreateStringOption("--verbosity");

        Option<string?> NetworkRequireAdapterTypeOption = CreateStringOption("--network-require-adapter-type");
        Option<bool?> networkRequireUpOption = CreateNullableBoolOption("--network-require-up");
        Option<string?> networkRequireFamilyOption = CreateStringOption("--network-require-family");
        Option<string?> networkAdapterFormatOption = CreateStringOption("--network-adapter-format");
        Option<string?> networkIpAddressFormatOption = CreateStringOption("--network-ip-address-format");
        Option<string?> networkXOption = CreateStringOption("--network-x");
        Option<string?> networkYOption = CreateStringOption("--network-y");
        Option<string?> networkSizeOption = CreateStringOption("--network-size");
        Option<string?> networkColorOption = CreateStringOption("--network-color");
        Option<bool?> networkRenderOption = CreateNullableBoolOption("--network-render");

        RootCommand root = new("BgRaster — per-output wallpaper renderer")
        {
            configOption,
            machineNameOption,
            textFormatOption, textSizeOption, textColorOption, textXOption, textYOption,
            bgColorOption, bgImageOption, bgFitOption, bgAlternatingOption, bgBorderOption, bgBorderColorOption,
            gridSizeOption, gridOddColorOption, gridEvenColorOption, gridStrokeOption,
            gridOffsetXOption, gridOffsetYOption, gridCoordinatesOption,
            circleSizeOption, circleXOption, circleYOption, circleColorOption, circleStrokeOption,
            crosshairLengthOption, crosshairXOption, crosshairYOption, crosshairColorOption, crosshairStrokeOption,
            logoSourceOption, logoXOption, logoYOption, logoAnchorXOption, logoAnchorYOption, logoWidthOption, logoHeightOption, logoOpacityOption,
            dryRunOption, noDiscoveryOption, outputsSkipUnspecifiedOption, outputDirOption, verbosityOption, continueAfterUnchangedOption,
            NetworkRequireAdapterTypeOption, networkRequireUpOption, networkRequireFamilyOption, networkAdapterFormatOption, networkIpAddressFormatOption,
            networkXOption, networkYOption, networkSizeOption, networkColorOption, networkRenderOption,
        };

        root.SetAction(async (parseResult, ct) =>
        {
            string? config = parseResult.GetValue(configOption);
            CliOverlay overlay = new()
            {
                MachineName = parseResult.GetValue(machineNameOption),
                TextFormat = parseResult.GetValue(textFormatOption),
                TextSize = parseResult.GetValue(textSizeOption),
                TextColor = parseResult.GetValue(textColorOption),
                TextX = parseResult.GetValue(textXOption),
                TextY = parseResult.GetValue(textYOption),
                BackgroundColor = parseResult.GetValue(bgColorOption),
                BackgroundImage = parseResult.GetValue(bgImageOption),
                BackgroundFit = parseResult.GetValue(bgFitOption),
                BackgroundAlternating = parseResult.GetValue(bgAlternatingOption),
                BackgroundBorder = parseResult.GetValue(bgBorderOption),
                BackgroundBorderColor = parseResult.GetValue(bgBorderColorOption),
                GridSize = parseResult.GetValue(gridSizeOption),
                GridOddColor = parseResult.GetValue(gridOddColorOption),
                GridEvenColor = parseResult.GetValue(gridEvenColorOption),
                GridStroke = parseResult.GetValue(gridStrokeOption),
                GridOffsetX = parseResult.GetValue(gridOffsetXOption),
                GridOffsetY = parseResult.GetValue(gridOffsetYOption),
                GridCoordinates = parseResult.GetValue(gridCoordinatesOption),
                CircleSize = parseResult.GetValue(circleSizeOption),
                CircleX = parseResult.GetValue(circleXOption),
                CircleY = parseResult.GetValue(circleYOption),
                CircleColor = parseResult.GetValue(circleColorOption),
                CircleStroke = parseResult.GetValue(circleStrokeOption),
                CrosshairLength = parseResult.GetValue(crosshairLengthOption),
                CrosshairX = parseResult.GetValue(crosshairXOption),
                CrosshairY = parseResult.GetValue(crosshairYOption),
                CrosshairColor = parseResult.GetValue(crosshairColorOption),
                CrosshairStroke = parseResult.GetValue(crosshairStrokeOption),
                LogoSource = parseResult.GetValue(logoSourceOption),
                LogoX = parseResult.GetValue(logoXOption),
                LogoY = parseResult.GetValue(logoYOption),
                LogoAnchorX = parseResult.GetValue(logoAnchorXOption),
                LogoAnchorY = parseResult.GetValue(logoAnchorYOption),
                LogoWidth = parseResult.GetValue(logoWidthOption),
                LogoHeight = parseResult.GetValue(logoHeightOption),
                LogoOpacity = parseResult.GetValue(logoOpacityOption),
                RenderDryRun = parseResult.GetValue(dryRunOption),
                RenderNoDiscovery = parseResult.GetValue(noDiscoveryOption),
                RenderOutputsSkipUnspecified = parseResult.GetValue(outputsSkipUnspecifiedOption),
                RenderOutput = parseResult.GetValue(outputDirOption),
                RenderVerbosity = parseResult.GetValue(verbosityOption),
                RenderContinueAfterUnchanged = parseResult.GetValue(continueAfterUnchangedOption),
                NetworkRequireAdapterType = parseResult.GetValue(NetworkRequireAdapterTypeOption),
                NetworkRequireUp = parseResult.GetValue(networkRequireUpOption),
                NetworkRequireFamily = parseResult.GetValue(networkRequireFamilyOption),
                NetworkAdapterFormat = parseResult.GetValue(networkAdapterFormatOption),
                NetworkIpAddressFormat = parseResult.GetValue(networkIpAddressFormatOption),
                NetworkX = parseResult.GetValue(networkXOption),
                NetworkY = parseResult.GetValue(networkYOption),
                NetworkSize = parseResult.GetValue(networkSizeOption),
                NetworkColor = parseResult.GetValue(networkColorOption),
                NetworkRender = parseResult.GetValue(networkRenderOption),
            };
            return await handler(config, overlay);
        });

        return root;

        static Option<string?> CreateStringOption(string alias) =>
            new(alias) { Description = GetByAlias(alias).HelpDescription };

        static Option<bool?> CreateNullableBoolOption(string alias) =>
            new(alias) { Description = GetByAlias(alias).HelpDescription };

        static CliOptionDefinition GetByAlias(string alias) =>
            GeneratedCliOptionCatalog.Definitions.First(definition =>
                string.Equals(definition.Alias, alias, StringComparison.Ordinal));
    }

    internal static string GetVersionString()
    {
        string version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.0.0";
        return $"BgRaster {version} {GetCopyright()}";
    }

    internal static string GetCopyright()
    {
        return Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyCopyrightAttribute>()
            ?.Copyright ?? "(C) 2026 Barjonas LLC";
    }
}
