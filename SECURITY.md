# Security policy

## Reporting a vulnerability

Report it privately. On the repository's
[Security tab](https://github.com/gameshowpro/BgRaster/security), choose
**Report a vulnerability**. Please do not open a public issue or pull request
for it.

If you cannot use that, contact the maintainer through
<https://barjonas.com/#contact>.

## Supported versions

Only the latest release gets security fixes. Older releases do not.

To upgrade, run `choco upgrade BgRaster -y` from an administrative shell if
you installed with Chocolatey, or `winget upgrade GameshowPro.BgRaster` if
you installed with winget. Otherwise, download the newest `BgRaster.zip` from
the [releases page](https://github.com/gameshowpro/BgRaster/releases). Both
package feeds are moderated, so a fix can reach the releases page a few days
before it reaches Chocolatey or winget. Installation is described under
[Quick start](https://bgraster.gameshow.pro/#quick-start).

## What counts as a vulnerability

BgRaster is a short-lived console application. It runs as the user who starts
it, needs no elevation, renders its wallpapers, assigns them and exits. It
opens no network listener and makes no network requests; it only reads the
local adapter configuration so that it can display it.

A config file is trusted input, like a script. Whoever writes it chooses which
images BgRaster reads, where it writes its output and what text appears on the
wallpaper. A config that makes BgRaster read a file its user can read, or show
that machine's addresses on screen, is working as documented.

The flaws that are in scope include:

- A crafted image, such as a background or logo, or a crafted config that
  makes BgRaster run code, or corrupt memory in it or in the bundled
  `libSkiaSharp.dll`.
- A way to make BgRaster delete, recycle or overwrite files other than its own
  output. It only recycles PNGs in its output folder whose names match the ones
  it generates.
- A way for another account on the same machine to change what BgRaster does
  for a user who did not choose that.
- A problem in an official release artifact or package itself.

Behavior the documentation describes is not a vulnerability. In particular,
running BgRaster with a config from someone you do not trust gives them the
control over it that the config file is meant to give.

## How reports are handled

BgRaster has a single maintainer. Reports are acknowledged and fixed on a
best-effort basis, and no response time is promised. A fix ships as a release
whose commit message says it is a security fix. Reporters who want credit are
credited there.

## Supply chain

Releases carry build provenance attestations for `BgRaster.zip`, the
Chocolatey `.nupkg`, `BgRaster.pdb`, and the extracted `BgRaster.exe` and
`libSkiaSharp.dll`. Check any of them, including a file installed by
Chocolatey or winget, with:

```pwsh
gh attestation verify BgRaster.exe --repo gameshowpro/BgRaster
```

See [Release contents](https://bgraster.gameshow.pro/developer/deployment/#release-contents)
for what each channel ships.
