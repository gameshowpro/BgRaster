# Only trust a machine-wide config that administrators control

Decided with the maintainer on 2026-10-20.

## Problem

Without `--config`, BgRaster searches for `config.toml` next to the executable,
then in `%ProgramData%\BgRaster`, `%LocalAppData%\BgRaster` and
`%AppData%\BgRaster`. On a stock Windows install any standard user can create a
folder under `C:\ProgramData` and owns what they create. So on a shared machine,
whoever creates `%ProgramData%\BgRaster` first can decide what every other
user's BgRaster run does (images read, output folder, wallpaper text) for any
user who has no config in an earlier location.

## Decisions (all three options, applied together)

1. **Trust check.** `%ProgramData%\BgRaster\config.toml` is used only if
   administrators control it (rule below). Otherwise it is skipped and a
   configuration warning is logged. The other three locations and an explicit
   `--config` path are not checked: the user chose them or owns them.
2. **Chocolatey install** creates `%ProgramData%\BgRaster` with admin-only
   write permissions if it does not exist. If it exists and fails the rule, the
   script leaves it alone and prints a warning. Uninstall does not remove it.
3. **New search order:** next to the executable, `%LocalAppData%\BgRaster`,
   `%AppData%\BgRaster`, then `%ProgramData%\BgRaster`. A user's own config now
   beats the machine-wide one.

## Canonical trust rule (slices 1, 2 and 3 must all match this)

Administrative SIDs:

- `S-1-5-32-544` BUILTIN\Administrators
- `S-1-5-18` NT AUTHORITY\SYSTEM
- `S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464` NT SERVICE\TrustedInstaller

Also ignore ACEs for `S-1-3-4` (OWNER RIGHTS), because the owner is required to
be administrative anyway. Always compare SIDs, never account names (Windows is
localized).

An ACE is **effective** if it is an allow ACE and is not inherit-only. Deny ACEs
are ignored (they can only remove rights). Generic rights in an ACE
(`GENERIC_ALL`, `GENERIC_WRITE`) count as granting every write-class right.

The **directory** `%ProgramData%\BgRaster` passes if:

- it is not a reparse point (junction or symlink),
- its owner is an administrative SID, and
- no effective ACE for a non-administrative SID grants `Delete`,
  `DeleteSubdirectoriesAndFiles`, `ChangePermissions` or `TakeOwnership`.

Note: creating files in the folder (`WriteData`/`AppendData` on a directory) is
deliberately allowed. A folder an administrator creates under `ProgramData`
inherits "Users: create files / create folders" by default. That is safe,
because any file a standard user creates there is owned by them and fails the
file rule. Rejecting it would make every default admin-created folder fail.

The **file** `config.toml` passes if:

- it is not a reparse point,
- its owner is an administrative SID, and
- no effective ACE for a non-administrative SID grants `WriteData`,
  `AppendData`, `WriteExtendedAttributes`, `Delete`, `ChangePermissions` or
  `TakeOwnership`.

A missing DACL (null DACL), or any failure to read owner or ACL, means it
**fails**. The config is used only if both the directory and the file pass.

Warning text when the machine-wide config is skipped (exact wording, `{0}` is
the full file path):

> Ignored '{0}': it is not controlled by administrators, so another account on this computer could have written it. See https://bgraster.gameshow.pro/developer/config-file-logic/#machine-wide-config

## Slices

### Slice 1: search order and trust check in the app

- **Files:** `src/Program.cs` (`GetDefaultConfigSearchPaths`,
  `ResolveConfigPath`, the call site in `RunAsync`), a new
  `src/Configuration/ConfigLocationTrust.cs` (or similar), and
  `tests/BgRaster.Tests/ProgramConfigTests.cs` plus a new test file for the
  evaluator.
- **Shape:** keep the ACL evaluation pure and unit-testable (input: is reparse
  point, owner SID, list of ACEs with SID, rights, allow/deny and inherit-only;
  output: pass or fail), with a thin Windows reader on top. The candidate list
  must say which entry needs the check (for example a small record with a
  `RequiresAdministratorControl` flag). `ResolveConfigPath` takes the trust
  predicate as a parameter so tests can inject it, and adds the warning to the
  existing `configurationWarnings` list.
- **Acceptance criteria:**
  - The search order is the new one, and `ProgramConfigTests` asserts it.
  - With a ProgramData candidate that exists but fails the predicate, resolution
    falls through to built-in defaults and the warning above is added verbatim.
    With one that passes, it is used. Tests cover both via an injected predicate.
  - The pure evaluator has tests for: admin-owned with default ProgramData
    inheritance (passes for the directory), non-admin owner (fails), Users
    granted `Delete`/`DeleteSubdirectoriesAndFiles`/`ChangePermissions`/
    `TakeOwnership` (fails), inherit-only CREATOR OWNER ACE (ignored), deny ACE
    (ignored), reparse point (fails), null DACL (fails), file with Users
    `WriteData` (fails).
  - One real-filesystem test: a fresh temp directory owned by the test user
    fails, unless the test process is elevated (skip or adapt in that case).
  - `dotnet test` passes, and `scripts/run-publish-smoke-test.ps1` passes
    (AOT publish with `TreatWarningsAsErrors`). If
    `System.Security.AccessControl` raises trim or AOT warnings, use P/Invoke
    (`GetNamedSecurityInfoW` and friends) through `LibraryImport`, matching
    `src/FileLifecycle/Interop`.
  - Code comments say why the check exists (one or two lines, not an essay).
- **Model:** opus. It is security-sensitive Windows ACL logic under NativeAOT
  constraints, where a subtle mistake fails open.

### Slice 2: Chocolatey install script

- **Files:** `build/chocolatey/BgRaster/chocolateyinstall.ps1`.
- **Behavior:**
  - If `%ProgramData%\BgRaster` does not exist: create it, set the owner to
    Administrators, disable inheritance, and grant SYSTEM and Administrators
    FullControl and Users ReadAndExecute (each container- and object-inherit).
  - If it exists: evaluate it with the directory rule, and if `config.toml` is
    present, also the file rule. If it fails, print a clear warning (BgRaster
    will ignore the config until an administrator fixes or recreates the folder)
    and change nothing. If it passes, change nothing.
  - Use SIDs, not names. Must work on Windows PowerShell 5.1, which is what
    Chocolatey runs. Keep the existing maintenance-fee message last.
- **Acceptance criteria:** the script parses
  (`[System.Management.Automation.Language.Parser]::ParseFile`). Extract the
  evaluation logic into a function and run it against a fresh user-owned temp
  directory (expect fail) and against `C:\Windows` (expect pass). Report both
  results. Do not run the creation path against the real `%ProgramData%`.
- **Model:** sonnet. It is a moderate PowerShell ACL task with a precise spec
  and little existing code to understand.

### Slice 3: documentation (after slice 1 lands)

- **Files:**
  - `docs/developer/config-file-logic.md`: new search order, and a new
    `## Machine-wide config` section (its anchor is used in the warning)
    explaining the rule in plain terms, why it exists, how to create the folder
    correctly (from an administrative shell, or install with Chocolatey), and
    that winget and zip installs do not create it.
  - `docs/cli-schema.md` line 16 and `docs/toml-schema.md` line 3: new order.
  - `docs/schemas/bgraster-config.schema.json`: the `--config`
    `defaultResolution` gets the new order. The `--machine-name`
    `defaultResolution` (around line 53) wrongly has the config-search text
    pasted in. Replace it with wording that matches the TOML-side
    `[render].machine-name` default resolution in the same file.
  - `README.md` "Generate default config file": new order. Also add a note that
    the `%ProgramData%` example must run from an administrative shell, or
    BgRaster will ignore that file when no `--config` is given.
  - `docs/plan.md` line 48: new order.
- **Acceptance criteria:** no remaining text anywhere (outside `site/`, `bin`,
  `obj`, `publish`) gives the old order. Run
  `scripts/generate-documentation.ps1`, then `mkdocs build --strict` with no
  warnings. Check the rendered HTML for config-file-logic, cli-schema and index,
  and confirm the `#machine-wide-config` anchor exists. Use en-US spelling.
- **Model:** sonnet. It is careful prose across several files, plus a generated
  docs pipeline to run and check.

Slices 1 and 2 touch disjoint files and run in parallel. Slice 3 runs after
slice 1, because the docs generator builds the project.

## Log

- Slice 2 (sonnet): accepted first time. The evaluator was checked on 5.1 and
  7.6: a temp dir fails, `C:\Windows` and `C:\ProgramData` pass. A 5.1-only
  `AceFlags -band` cast bug was found and fixed. The creation path
  (`New-AdminControlledDirectory`) is untested because no elevated shell was
  available, and needs one elevated run against a scratch path. Non-allow and
  callback ACE types are skipped; slice 1 should match.
- Slice 1 (opus): accepted first time, 193/193 tests (re-run by the
  orchestrator). It uses `LibraryImport` P/Invoke rather than
  `System.Security.AccessControl`, reading the reparse flag, owner and DACL
  from one handle opened with `FILE_FLAG_OPEN_REPARSE_POINT`. The AOT binary
  logged the exact warning against a fake user-owned ProgramData config. An ACE
  of an unknown type fails the whole check, and an empty DACL passes.
- Orchestrator aligned slice 2 with that: a non-qualified ACE now fails the
  check instead of being skipped (one-line edit).
- Found during slice 1, outside this task: `scripts/run-publish-smoke-test.ps1`
  without `-SkipPublish` fails on clean HEAD with NETSDK1207. Its global
  `/p:PublishAot=true` reaches the netstandard2.0 SchemaMetadataGenerator
  project. CI is unaffected because it passes `-SkipPublish`.
- Slice 3 (sonnet): accepted first time. The generator and `mkdocs build
  --strict` are clean, and the `#machine-wide-config` anchor is present in the
  rendered HTML. The `--machine-name` defaultResolution is fixed from the
  common schema's `[render].machine-name` text. Tests re-run after the schema
  change: 193/193.
