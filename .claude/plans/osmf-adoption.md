# Fold BgRaster into the GameshowPro Open Source Maintenance Fee scheme

Modeled on Suede's adoption (`V:\Repos\GameshowPro\Suede`, commit `c5e0048`,
plan `Suede/.claude/plans/osmf-adoption.md`, live page
`Suede/docs/maintenance-fee.md`). Decided with the maintainer on 2026-10-17:

- **License:** relicense from MIT to **MPL-2.0**. Hamish Barjonas is the only
  committer, so relicensing is clean. Past releases stay MIT.
- **Fee model:** identical to Suede's current page: OSMF EULA template v1.1;
  Small (fewer than 10 employees) USD 10/month or USD 120 one-time annual,
  Medium (10 to 100) USD 40 / 480, Large (more than 100) USD 60 / 720;
  threshold USD 10,000 annual gross revenue; non-revenue use exempt; waiver
  exemption for anyone who has engaged Barjonas LLC; honor system.
- **Per application:** the fee is charged **per application**. BgRaster's fee
  covers BgRaster only; Suede has its own. "Two app" and "three app" tiers
  will be added later. Until then the docs tell multi-app users to ask
  (GitHub Sponsors allows only one active monthly tier per sponsor, so two
  separate monthly sponsorships are not possible today).
- **Who owes it:** whoever owns the computers that run BgRaster (Suede's
  ownership rule, with "appliances" replaced by "computers").
- **Eduforcement only:** notice in `--help` and `--version`, a one-time
  console notice per Windows user, the Chocolatey install message, winget
  `InstallationNotes`, the release zip (EULA, license, short README) and the
  GitHub release body, plus README and docs. Nothing is checked or sent.
- **Trademark and contributions:** mirror Suede: `TRADEMARKS.md` claiming
  "BgRaster", and the contribution sign-off and sublicensing grant.
- **Effective:** every official release that ships `OSMFEULA.txt`. No date
  named in the docs. `version.json` is not changed by this work.

## Canonical facts every slice uses

- Fee page: `docs/maintenance-fee.md`, published at
  `https://bgraster.gameshow.pro/maintenance-fee/`. Headings and anchors exactly
  as Suede's: `who-pays`, `what-it-costs`, `how-to-pay`, `exemptions`,
  `how-long-the-fee-applies`, `building-from-source-instead`, `license`,
  plus `## Questions`.
- Sponsor URL: `https://github.com/sponsors/gameshowpro?metadata_project=bgraster&metadata_source=<src>`
  with `<src>` one of `docs`, `readme`, `app`, `chocolatey`, `winget`, `release`.
- OSMF site: `https://opensourcemaintenancefee.org`. EULA on GitHub:
  `https://github.com/gameshowpro/BgRaster/blob/main/OSMFEULA.txt`.
  Contact: `https://barjonas.com/#contact`.
- **Official builds** = `BgRaster.zip` on GitHub Releases, the Chocolatey
  package `BgRaster`, and the winget package `GameshowPro.BgRaster` (which
  installs that same zip). A binary you compile yourself is not official.
- Where the EULA ships: the repo root, the zip root (`OSMFEULA.txt`), and the
  Chocolatey package (copied into `tools\` next to `BgRaster.exe` by the
  install script).
- One-line summary: "BgRaster participates in the Open Source Maintenance Fee:
  the source is free under MPL-2.0, and organizations that use the official
  builds as part of revenue-generating activities pay a small monthly fee that
  funds maintenance."
- Console notice (Slice 3), exact text, ASCII only:
  ```
  BgRaster participates in the Open Source Maintenance Fee. The source is free
  under MPL-2.0. Organizations that use official BgRaster builds as part of
  revenue-generating activities pay a small monthly fee that funds maintenance.
  Who pays and how: https://bgraster.gameshow.pro/maintenance-fee/
  Shown once per Windows user. Nothing is checked or sent.
  ```
  Marker file: `%LocalAppData%\BgRaster\maintenance-fee-notice`. Creating that
  file in advance (any content) suppresses the notice, which the docs tell
  fleet administrators.
- `--help` footer and `--version` second line, exact text:
  `Open source under MPL-2.0. Official builds participate in the Open Source Maintenance Fee: https://bgraster.gameshow.pro/maintenance-fee/`
- Package install message (Chocolatey `Write-Host`, winget `InstallationNotes`):
  ```
  BgRaster participates in the Open Source Maintenance Fee. Organizations that
  use it as part of revenue-generating activities pay a small monthly fee that
  funds maintenance. Who pays and how: https://bgraster.gameshow.pro/maintenance-fee/
  ```

## Repo facts

- The working tree has the maintainer's uncommitted edits in `README.md` and
  `docs/developer/coding-standards.md` ("Game Show Pro" -> "GameshowPro").
  Keep them; never revert or `git checkout` those files.
- `docs/index.md` is generated from `README.md` by
  `scripts/generate-documentation.ps1` and is gitignored. Edit the README,
  never `docs/index.md`. README `> [!NOTE]` blocks become Material
  admonitions; `### Option N` between `<!-- docs-tabs-start/end -->` become tabs.
- Every C# file starts `// SPDX-License-Identifier: MIT` then
  `// Copyright © 2026 Barjonas LLC` (106 files; some have a BOM or CRLF).
- Build and test: `dotnet build tests/BgRaster.Tests/BgRaster.Tests.csproj -c Release -r win-x64 /p:TreatWarningsAsErrors=true`
  then `dotnet test tests/BgRaster.Tests/BgRaster.Tests.csproj -c Release -r win-x64 --no-build`.
- Docs: `./scripts/generate-documentation.ps1`, then `mkdocs build --strict`,
  then inspect the rendered HTML under `site/` (standing maintainer rule).
- Code and docs are en-US.

---

## Slice 1: Legal files and relicense

**Model:** sonnet. Mostly mechanical, plus adapting two short Suede documents in their established voice.

**Files:** `LICENSE`, `OSMFEULA.txt` (new), `TRADEMARKS.md` (new),
`.github/CONTRIBUTING.md`, `.github/FUNDING.yml`, all `*.cs` SPDX headers under
`src/` and `tests/` (including any header text emitted by
`src/Generators/**` or `scripts/generate-network-simulator.ps1`),
`.editorconfig` (comment at line ~203 only), `docs/plan.md` (line 8).

**Steps:**

1. `LICENSE`: replace with `Suede/LICENSE` verbatim (the SPDX MPL-2.0 text).
2. `OSMFEULA.txt`: copy `Suede/OSMFEULA.txt`, replacing `Suede` with
   `BgRaster` everywhere (header line, party name, software name) and the
   payment-terms URL with `https://bgraster.gameshow.pro/maintenance-fee/`.
   Change nothing else. Keep the line wrap.
3. `TRADEMARKS.md`: adapt `Suede/TRADEMARKS.md` for BgRaster. Replace the
   name and examples. Keep the structure and voice. Make no claim about
   GameshowPro itself.
4. `.github/CONTRIBUTING.md`: keep the existing first paragraph, then add the
   license-of-contributions section adapted from `Suede/CONTRIBUTING.md`
   (MPL-2.0 inbound = outbound, DCO `Signed-off-by`, the sublicensing grant
   and why: official builds ship under a separate maintenance-fee EULA).
5. `.github/FUNDING.yml`: match Suede's (`custom: ["https://paypal.me/barjonas"]`).
6. Change every `// SPDX-License-Identifier: MIT` to
   `// SPDX-License-Identifier: MPL-2.0`. Preserve each file's BOM and line
   endings: no whole-file rewrites that normalize them. Also update any
   generator or script that emits this header into generated code.
7. `.editorconfig` comment and `docs/plan.md` line 8: MIT -> MPL-2.0.

**Acceptance criteria:**

- `git grep -n "SPDX-License-Identifier: MIT"` returns nothing;
  `git grep -c "SPDX-License-Identifier: MPL-2.0"` covers the same 106 files.
- `git diff --stat` on `.cs` files shows exactly one line changed per file,
  with no line-ending churn.
- `OSMFEULA.txt` contains no `Suede` and no `<` placeholders, and does contain
  the widened exemption sentence and the BgRaster payment-terms URL.
- The solution builds and tests pass.

---

## Slice 2: Packaging and release

**Model:** sonnet. CI and packaging edits with every text given here. A mistake breaks a release, which can be recovered.

**Files:** `build/chocolatey/BgRaster/LICENSE.txt`,
`build/chocolatey/BgRaster/publish.nuspec`,
`build/chocolatey/BgRaster/chocolateyinstall.ps1`,
`build/winget/GameshowPro.BgRaster.locale.en-US.yaml`,
`build/release/README.txt` (new), `.github/workflows/ci.yml`.

**Steps:**

1. Chocolatey `LICENSE.txt`: keep the `From: .../LICENSE` first line and
   blank line, and replace the body with the MPL-2.0 text from the repo's new
   `LICENSE`.
2. `publish.nuspec`: add `<file src="..\publish\staging\OSMFEULA.txt" target="binaries" />`.
   In the description, replace "Support & Donation" with a "Maintenance fee"
   section: the one-line summary, the fee-page link and the sponsor URL
   (`metadata_source=chocolatey`). Keep a brief donations line. Leave
   `requireLicenseAcceptance` unset (eduforcement only).
3. `chocolateyinstall.ps1`: after the success line, `Write-Host` the package
   install message (canonical text above).
4. winget locale manifest: `License: MPL-2.0`. Add `InstallationNotes:` with
   the package install message as a `|-` block. Do **not** add
   `Agreements`, which would force `--accept-package-agreements` on every
   install. Check that the 1.9.0 defaultLocale schema allows
   `InstallationNotes` (it does since 1.4.0).
5. `build/release/README.txt` (new, plain text, ~12 lines, CRLF): what
   BgRaster is, `BgRaster.exe` and `libSkiaSharp.dll` must stay together,
   docs URL, the console-notice paragraph (canonical text without its last
   line), license MPL-2.0 (`LICENSE.txt`), fee terms (`OSMFEULA.txt`).
6. `ci.yml`, "Stage release artifacts" step: copy repo `OSMFEULA.txt` and
   `build/release/README.txt` into staging, and make the zip contain
   `BgRaster.exe`, `libSkiaSharp.dll`, `LICENSE.txt`, `OSMFEULA.txt`,
   `README.txt`. The staging `LICENSE.txt` already comes from the Chocolatey
   folder. Its `From:` line is fine in the zip, so reuse it rather than adding
   a second copy. Update the step's name and comments.
7. `ci.yml`, "Create or update GitHub release": add a `body:` with the fee
   paragraph below. Keep `generate_release_notes: true`, which appends:
   ```
   BgRaster participates in the [Open Source Maintenance Fee](https://bgraster.gameshow.pro/maintenance-fee/).
   The source is free under MPL-2.0. Organizations that use these official builds as part of
   revenue-generating activities pay a small monthly fee that funds maintenance. The terms are in
   `OSMFEULA.txt`, inside `BgRaster.zip`.
   [Pay via GitHub Sponsors](https://github.com/sponsors/gameshowpro?metadata_project=bgraster&metadata_source=release)
   ```
8. Add `'OSMFEULA.txt'` and `'LICENSE'` to the `app` paths filter in `ci.yml`.
9. Do **not** run `dotnet build`/`publish` (another slice builds at the same
   time). Validate the staging logic by extracting the step's PowerShell into
   a scratch script and running it against a fake `publish/win-x64` with
   dummy files under a temp directory (the step's paths are relative, so run
   it from a scratch copy, not the repo). Then list the zip entries.

**Acceptance criteria:**

- The zip built by the extracted staging logic contains exactly the five
  files listed in step 6.
- `ci.yml` and the winget YAML parse (for example with
  `python -c "import yaml,sys; yaml.safe_load(open(sys.argv[1]))"`).
- nuspec is well-formed XML and lists `OSMFEULA.txt`.
- No `MIT` remains in `build/`.

---

## Slice 3: Console notice, help and version text

**Model:** sonnet. A small, well-specified feature with tests. A crash here runs at every login, so the spec requires failing safe.

**Files:** `src/Licensing/MaintenanceFeeNotice.cs` (new; adjust the folder if
an existing one fits better), `src/Program.cs`,
`src/Configuration/CliBinding.cs`,
`tests/BgRaster.Tests/MaintenanceFeeNoticeTests.cs` (new), and any existing
test that asserts exact help or version output.

**Steps:**

1. `MaintenanceFeeNotice` (internal static): `const string Text` (canonical
   console notice), `const string Footer` (canonical help/version line),
   `GetMarkerPath()` ->
   `Path.Combine(Environment.GetFolderPath(LocalApplicationData), "BgRaster", "maintenance-fee-notice")`,
   and a testable core
   `internal static bool TryShowOnce(TextWriter writer, string markerPath, bool interactive)`:
   it returns false without writing when `!interactive` or the marker file
   exists. Otherwise it writes a blank line then `Text` to `writer`, creates
   the directory, and writes the marker (UTC ISO-8601 timestamp and the
   version). Any `IOException`/`UnauthorizedAccessException` is swallowed:
   the notice may then show again next run, which is acceptable. It must
   never throw.
2. `Program.Main`: the notice shows only when a run actually executed
   (`RunAsync` was entered; track it with a static flag), not for
   `--help`/`--version`/parse errors. Show it after `InvokeAsync` returns,
   for any exit code, so it ends up below the log output. Write to
   `Console.Error`. `interactive` = `!Console.IsOutputRedirected && !Console.IsErrorRedirected`.
   Notice errors never change the exit code.
3. `--version`: keep line 1 exactly as today and add `Footer` as line 2.
4. `WriteCategorizedHelp`: add a blank line then `Footer` at the end, dimmed
   when color is on, like the other muted text.
5. Tests: marker absent + interactive -> writes text and creates marker;
   second call -> writes nothing; non-interactive -> writes nothing and
   creates no marker; marker path in a read-only or invalid location -> no
   throw. Use temp directories only. Never touch the real `%LocalAppData%`.
   Update any test that snapshots help or version output.
6. Build with warnings as errors and run the full test suite. Then run
   `dotnet run --project src/BgRaster.csproj -c Debug -- --help` and
   `-- --version` and check the footer appears.
   Do not do a real run that assigns wallpaper.

**Acceptance criteria:**

- Build has no warnings, and all tests pass, including the new ones.
- `--help` and `--version` show the footer, and line 1 of `--version` is
  unchanged.
- No code path lets an exception from the notice escape, and the notice is
  never written to stdout.

---

## Slice 4: Documentation site and README

**Model:** opus. A long public page with legal and commercial weight, adapted rather than copied, and README changes that feed the generated docs home page.

**Files:** `docs/maintenance-fee.md` (new), `docs/assets/osmf/*` (copied from
`Suede/docs/assets/osmf/`: three SVGs and `SOURCE.md`), `mkdocs.yml`,
`docs/assets/stylesheets/extra.css`, `README.md`,
`docs/developer/deployment.md`.

**Steps:**

1. `docs/maintenance-fee.md`: adapt `Suede/docs/maintenance-fee.md` section by
   section. Same front matter, H1, logo lines, headings and order. Changes:
   - Official packages -> official builds (the three channels above). The
     EULA locations are the zip root and the Chocolatey `tools` folder.
   - Appliances -> computers (workstations, media servers, playback and
     graphics machines, rental rigs). Ownership rule unchanged.
   - Revenue-generating examples fit BgRaster's users: broadcast, live
     events, venues, rental and staging companies, installations.
   - New paragraph in "Who pays", after "per organization": the fee is per
     application. It covers BgRaster; other GameshowPro applications such
     as Suede have their own; anyone using more than one should
     [ask](https://barjonas.com/#contact) about paying for them together.
     Do not mention future bundle tiers.
   - Sponsor links use `metadata_project=bgraster&metadata_source=docs`.
   - "How long the fee applies": the note's reasoning in BgRaster terms
     (machines are imaged once and left alone, and BgRaster runs from login
     scripts nobody revisits).
   - "Building from source instead": .NET 10 SDK plus the Visual Studio
     "Desktop development with C++" workload (NativeAOT needs the MSVC
     linker); `git clone https://github.com/gameshowpro/BgRaster.git`,
     `cd BgRaster`, the README's `dotnet publish` line and output path. A
     self-built exe still shows the console notice once; the notice is worded
     for official builds, so it can be ignored. Nothing is held back from
     self-builds.
   - "License": BgRaster, and links to `TRADEMARKS.md` and
     `.github/CONTRIBUTING.md` on GitHub.
   - Questions: adapt all five. Replace the UI-notice question with "Can I
     turn off the console notice?": it shows once per Windows user, after a
     run, only in an interactive console; to suppress it across a fleet,
     create `%LocalAppData%\BgRaster\maintenance-fee-notice` for each user
     (for example from the same login script). Maintenance means keeping
     BgRaster working on current Windows, .NET and display drivers.
2. Copy the four OSMF asset files byte-identically and add Suede's
   `.md-typeset img.osmf-wide` rule to `extra.css`.
3. `mkdocs.yml`: nav `- Maintenance fee: maintenance-fee.md` after
   Troubleshooting; add `attr_list` and `md_in_html` to `markdown_extensions`
   (needed for `{ .osmf-wide }` and `.md-button`); set
   `copyright: '© 2026 Barjonas LLC · MPL-2.0 · <a href="https://bgraster.gameshow.pro/maintenance-fee/">Maintenance fee</a>'`.
4. `README.md` (keep the maintainer's uncommitted edits):
   - Badges: License badge -> `https://img.shields.io/badge/license-MPL--2.0-blue?style=flat-square`
     linking `LICENSE`. Add Suede's OSMF badge (color `37660D`) linking
     `OSMFEULA.txt`. Keep the Sponsor and PayPal badges.
   - New `## Open Source Maintenance Fee` after `## Summary`, modeled on
     Suede's README section: `<picture>` with
     `https://bgraster.gameshow.pro/assets/osmf/...`, the required first line,
     the official builds named (GitHub release zip, Chocolatey, winget),
     ownership rule, tiers, per-application sentence, exemptions,
     maintenance not support, the build-from-source option, the sponsor link
     (`metadata_source=readme`) and the full-details link.
   - Just before `<!-- docs-tabs-start -->`: a `> [!NOTE]` saying options
     1-3 install official builds, which the maintenance fee covers for
     revenue-generating use, with a link to the fee page.
   - Option 4: fix the clone to `https://github.com/gameshowpro/BgRaster.git`
     and `cd BgRaster`.
   - `## License`: rewrite after Suede's README License section, for
     BgRaster and with `.github/CONTRIBUTING.md`.
5. `docs/developer/deployment.md`: add a short "Release contents" section
   covering what the zip and the Chocolatey package contain, including
   `OSMFEULA.txt`. Every published release carries the EULA, so a release is
   what makes the fee apply to a version. Note that the winget manifest
   carries `InstallationNotes` and deliberately no `Agreements`.
6. Run `./scripts/generate-documentation.ps1`, `mkdocs build --strict`, and
   inspect `site/maintenance-fee/index.html` and `site/index.html`. Check
   that both logos and the button render, that the anchors exist, that the
   admonitions render, and that no raw `{ .` or `!!!` text appears. If
   `generate-documentation.ps1` builds the app, wait until Slice 3 is done.

**Acceptance criteria:**

- The fee page has the seven headings plus Questions, in Suede's order, and
  states the per-application rule, the ownership rule, the tiers, the
  threshold, the duration rule, the waiver exemption, the self-build path and
  the notice opt-out.
- `mkdocs build --strict` passes. The rendered HTML shows the logo images, the
  button, the nav entry and the footer link.
- No "MIT" in `README.md`, `docs/` (except generated or historical files) or
  `mkdocs.yml`.

---

## Order

Slice 1 first (it touches every `.cs` file). Then Slice 3. Then Slices 2 and 4
in parallel: their files are disjoint, and Slice 2 does not build.

## Validation and escalation log

| Slice | Model | Result |
|---|---|---|
| 1 | sonnet | Accepted first pass. 106 SPDX headers swapped without line-ending churn; build and 139 tests green. Fixed by hand: relative links in `.github/CONTRIBUTING.md` (`../LICENSE`, `../OSMFEULA.txt`). |
| 2 | sonnet | Accepted first pass. Staging simulated against dummy binaries: zip holds the five files; ci.yml, winget YAML and nuspec parse. Not run in real CI. |
| 3 | sonnet | Accepted first pass. 146 tests (7 new) green with warnings as errors; --help/--version footer checked. Notice not exercised by a real wallpaper run. |
| 4 | opus | Accepted first pass. Strict docs build green, rendered HTML checked. Fixed by hand: dropped Suede's web-UI sentence from `docs/assets/osmf/SOURCE.md` and excluded it from the site; added `metadata_project=bgraster` to the README sponsor badge and the mkdocs footer link. |

Outside the repo (for the maintainer): add a BgRaster fee tier set on GitHub
Sponsors (or decide how BgRaster-only payers choose a tier while Suede's tiers
exist), push the next release so the EULA ships, and list BgRaster on the OSMF
participating-projects page.
