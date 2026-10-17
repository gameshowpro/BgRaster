# Deployment Notes

## Summary

This document tracks deployment and release-time considerations that are currently known for BgRaster.

## Tomlyn IL2104 Under NativeAOT + Trimming

### Symptom

Release publish with NativeAOT and full trimming can fail when warnings are treated as errors:

- `dotnet publish ... /p:PublishAot=true /p:TreatWarningsAsErrors=true`
- Warning promoted to error: `IL2104` for Tomlyn.

### What it means

`IL2104` is an aggregate trimmer warning emitted for an assembly (Tomlyn). The detailed warnings indicate reflection-based APIs that are not fully annotated for trimming.

### Impact assessment

In this project, Tomlyn is used through `Toml.ToModel(...)` and manual `TomlTable` extraction, not direct POCO model deserialization. That lowers risk, but does not eliminate it.

### Mitigation now in place

`src/BgRaster.csproj` sets:

- `<WarningsNotAsErrors>$(WarningsNotAsErrors);IL2104</WarningsNotAsErrors>` for non-Debug configurations.

This keeps strict warnings-as-errors behavior for other warnings while preventing this known third-party trim warning from blocking release publish.

### Validation strategy

Use published-binary smoke validation in CI and locally:

- Script: `scripts/run-publish-smoke-test.ps1`
- CI step: `Smoke test published binary`

Smoke test coverage includes:

- Executable startup (`--help`)
- Real render path with a temporary config
- `no-discovery` + `no-assignment` path
- Verification that expected output file is produced

## Release contents

Every official release reaches users through three channels, and every one of them carries the Open Source Maintenance Fee EULA, `OSMFEULA.txt`:

- **`BgRaster.zip` on GitHub Releases** contains `BgRaster.exe`, `libSkiaSharp.dll`, `LICENSE.txt` (MPL-2.0), `OSMFEULA.txt` and a short `README.txt`, all at the root. The release job in `.github/workflows/ci.yml` stages them. `README.txt` comes from `build/release/`, and `LICENSE.txt` is the copy in `build/chocolatey/BgRaster/`. `BgRaster.pdb` is attached to the release separately.
- **The Chocolatey package `BgRaster`** carries the same binaries, `LICENSE.txt`, `VERIFICATION.txt` and `OSMFEULA.txt` in its `binaries` folder. `chocolateyinstall.ps1` copies them into `tools\`, next to `BgRaster.exe`, and then prints the maintenance-fee message.
- **The winget package `GameshowPro.BgRaster`** installs that same zip. Its locale manifest carries the maintenance-fee message as `InstallationNotes`, which winget shows after installing. It deliberately has no `Agreements`: those would force `--accept-package-agreements` on every install, and the fee works on the honor system.

Because every published release carries the EULA, publishing a release is what makes the fee apply to that version. Do not publish one without `OSMFEULA.txt`. Builds compiled from source are not official builds, and the EULA does not apply to them. See [Maintenance fee](../maintenance-fee.md#building-from-source-instead).
