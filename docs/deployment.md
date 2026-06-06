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