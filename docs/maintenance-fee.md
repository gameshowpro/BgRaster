---
description: BgRaster is open source under MPL-2.0 and participates in the Open Source Maintenance Fee. Who pays, what it costs, how to pay, and how to build it yourself instead.
---

# Maintenance fee and license

![Open Source Maintenance Fee](assets/osmf/osmf-wide-on-light.svg#only-light){ .osmf-wide }
![Open Source Maintenance Fee](assets/osmf/osmf-wide-on-dark.svg#only-dark){ .osmf-wide }

BgRaster participates in the Open Source Maintenance Fee: the source is free under MPL-2.0, and organizations that use the official builds as part of revenue-generating activities pay a small monthly fee that funds maintenance.

The fee follows the [Open Source Maintenance Fee](https://opensourcemaintenancefee.org) (OSMF) EULA template, version 1.1. The exact terms are in [`OSMFEULA.txt`](https://github.com/gameshowpro/BgRaster/blob/main/OSMFEULA.txt), which also ships inside every official build. This page explains them in plain words, and it is where the EULA points for its payment terms: the tiers, how to pay, and how long the fee applies.

Nothing in BgRaster checks whether you have paid. The console shows a notice once per Windows user, this page explains the rest, and the system works on trust. Nothing is sent anywhere.

## Who pays

The fee applies to an organization or a self-employed professional with **at least USD 10,000 in annual gross revenue** that uses BgRaster's **official builds** as part of revenue-generating activities. The official builds are `BgRaster.zip` on the [GitHub releases page](https://github.com/gameshowpro/BgRaster/releases), the Chocolatey package [`BgRaster`](https://community.chocolatey.org/packages/BgRaster), and the winget package `GameshowPro.BgRaster`, which installs that same zip.

Revenue-generating use is the ordinary professional case:

- a broadcast, production or live event that runs BgRaster on its control room, playback or graphics machines;
- a venue, arena or attraction whose display and media servers run BgRaster;
- a rental or staging company whose machines go out with BgRaster on them;
- an installation sold to a client, or operated on a client's behalf.

Government agencies are treated like any other organization.

Nobody else owes anything. Hobbyists, schools, churches and charities, evaluation and development use, and anyone below the revenue threshold use the official builds free.

The fee is **per organization**, not per computer. One payment covers every computer the organization runs BgRaster on: workstations, media servers, playback and graphics machines, and rental rigs alike.

The fee is also **per application**. It covers BgRaster. Other GameshowPro applications, such as [Suede](https://suede.gameshow.pro/), have their own. If you use more than one, [ask](https://barjonas.com/#contact) about paying for them together.

It is owed once, by **whoever owns the computers** that run BgRaster: the organization or self-employed professional whose machines they are, or the rental company for rented equipment. Productions are usually a chain of companies subcontracting each other, and every link earns revenue from the same show, so ownership is what decides. Companies further along the chain, who receive a service from those computers without owning them, owe nothing for them.

- A freelancer who brings their own computers pays once, and that covers every job and every client.
- A freelancer operating a services company's computers owes nothing for that work. The services company pays.
- A system sold and installed at a venue belongs to the venue, so the venue pays. A system an integrator rents out or operates as a service stays the integrator's, so the integrator pays.

Anyone may pay on behalf of the owner, for example a services company covering the freelancers it hires, or an integrator covering a client as part of the deal. If your arrangement does not fit, [ask](https://barjonas.com/#contact).

## What it costs

The tier is set by the number of people in the organization that owns the computers. [How to count them](#how-do-we-count-employees) is answered below. Pay monthly, or once for a year.

| Organization size | Monthly | Annual (one-time) |
|---|---|---|
| Small: fewer than 10 employees | USD 10 | USD 120 |
| Medium: 10 to 100 employees | USD 40 | USD 480 |
| Large: more than 100 employees | USD 60 | USD 720 |

You choose your own tier, on the honor system. Nothing asks for proof and nothing verifies it.

## How to pay

Payment goes through [GitHub Sponsors](https://github.com/sponsors/gameshowpro?metadata_project=bgraster&metadata_source=docs).

1. Open the [sponsor page](https://github.com/sponsors/gameshowpro?metadata_project=bgraster&metadata_source=docs) while signed in to GitHub. Sponsor from your organization's GitHub account rather than a personal one, so the payment and its receipts carry the organization's name.
2. Pick the maintenance-fee tier named for your organization's size, monthly or one-time.
3. Pay by card, or by invoice if your accounts department needs one. GitHub's invoiced billing works in three steps: enable it for your organization, pay the invoice GitHub sends, then sponsor from the resulting balance.

Receipts come from GitHub.

[Pay via GitHub Sponsors](https://github.com/sponsors/gameshowpro?metadata_project=bgraster&metadata_source=docs){ .md-button .md-button--primary }

## Exemptions

You owe nothing if any of these applies:

- **Below the threshold.** Your organization's annual gross revenue is less than USD 10,000.
- **Non-revenue use.** You use BgRaster for evaluation, development, education, worship, charity or a hobby.
- **You have engaged Barjonas LLC.** Organizations that have engaged Barjonas LLC for custom work or support, now or in the past, are exempt. They hold written confirmation of it. If you think this applies to you and have nothing in writing, ask for it.
- **You pay for support.** Anyone paying a separate support or maintenance contract for BgRaster is already funding its maintenance and is not charged twice.

For a waiver, a support contract or anything this page does not answer, [get in touch](https://barjonas.com/#contact).

## How long the fee applies

The fee applies **for as long as any official BgRaster build is in revenue-generating use in your organization**, whether or not you upgrade it.

!!! note "Why this rule is written down"
    Display machines are often imaged once and left alone for years, and BgRaster runs from a login script or scheduled task that nobody revisits. That is the point of it. A rule that tied the fee to upgrades would leave everyone guessing whether a machine nobody has touched since it was imaged still counts. It does. Running the build is what counts, not updating it, and the rule is stated here precisely so that nobody has to guess.

Paying entitles you to every release, so there is no reason to stay on an old version to avoid anything.

Stopping payment revokes nothing. What is already installed keeps working, and nothing is restricted beyond what the MPL already says. It simply means you should stop using the official builds for revenue-generating work, and either pay again or switch to a build of your own.

The fee covers official builds that carry the EULA. The release zip has it at its root as `OSMFEULA.txt`, and the Chocolatey package installs it into its `tools` folder, next to `BgRaster.exe`.

## Building from source instead

The fee is on the convenience of a prebuilt binary, not on the software. Anyone may build BgRaster from source and use that build for any purpose, revenue-generating or not, without paying. A binary you compile yourself is not an official build, so the EULA does not apply to it. Chocolatey and winget do not install it either, so copy it to your computers yourself.

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and Visual Studio, or the Visual Studio Build Tools, with the **Desktop development with C++** workload. NativeAOT needs its MSVC linker.

```pwsh
git clone https://github.com/gameshowpro/BgRaster.git
cd BgRaster
dotnet publish src/BgRaster.csproj -c Release -r win-x64 /p:PublishAot=true
```

The published binary lands in `src/bin/Release/net10.0/win-x64/publish/BgRaster.exe`. Keep `libSkiaSharp.dll` from the same folder next to it, as in the release zip. See [Deployment](developer/deployment.md#release-contents) for what the official zip contains.

A self-built BgRaster still shows the console notice once per Windows user. The notice is worded for official builds, so you can ignore it, or [turn it off](#can-i-turn-off-the-console-notice).

What you lose is convenience: the official builds arrive ready to run, and Chocolatey and winget can upgrade them for you. Building your own means doing that yourself, each time you want a new version. Nothing else is held back. No feature is reserved for the official builds.

## License

BgRaster's source code is open source under the [Mozilla Public License 2.0](https://github.com/gameshowpro/BgRaster/blob/main/LICENSE) (MPL-2.0), an OSI-approved license. In plain words:

- You may use, modify, embed and redistribute BgRaster, commercially or not.
- If you distribute a modified version, you must publish your changes to BgRaster's own files under the MPL.
- Your own code stays yours. Combining it with BgRaster, in the same product or the same package, does not put it under the MPL, as long as it lives in its own files.

The maintenance fee is **not a license fee**. The EULA governs only the official builds, and where it conflicts with the MPL, the MPL wins. It adds no restriction to the source, to binaries you build, or to redistribution.

The name is a separate matter. "BgRaster" is a trademark of Barjonas LLC, and the MPL grants no rights in it. Modify the software as much as you like, but naming a modified version "BgRaster", or presenting it as official, needs written permission. [TRADEMARKS.md](https://github.com/gameshowpro/BgRaster/blob/main/TRADEMARKS.md) lists what is fine without asking.

Contributions are accepted under the MPL, with a sign-off and a grant that keeps future licensing options open. [CONTRIBUTING.md](https://github.com/gameshowpro/BgRaster/blob/main/.github/CONTRIBUTING.md) explains what is asked for and why.

## Questions

### We pay, but a bug we reported is not fixed

The fee funds maintenance: keeping BgRaster working on current Windows, current .NET and current display drivers, fixing what breaks, and shipping releases. It is not a support contract and does not buy a response time or a fix for a particular problem. If you need that, [support contracts are available](https://barjonas.com/#contact), and holding one exempts you from the fee.

### Do we pay per computer?

No. One payment covers every computer your organization runs BgRaster on. It covers BgRaster only: other GameshowPro applications have their own fee.

### How do we count employees?

Count the people who regularly work for the organization that owns the computers, as it is across a typical year:

- **Everyone on the payroll counts as one,** full-time or part-time. Count heads, not hours.
- **Owners and partners who work in the business count.**
- **Freelancers who work mainly for you count.** Someone who spends most of their working time on your jobs is part of your team, whatever their contract says. Freelancers you book job by job, who also work for others, do not.
- **Use a typical month, not your busiest week.** Crew brought in for a single show or season do not count.
- **Count the whole group.** If the owner belongs to a group of companies, count the group, not just the company whose name is on the equipment.
- **A self-employed professional working alone counts as one,** and pays the small tier.

When your size changes, move to the new tier at your next payment. If you are close to a border, use your best estimate. Nobody checks.

### We only use BgRaster on one show a year

That is still revenue-generating use. If your organization is small, the fee is USD 10 a month, or USD 120 for the year.

### Our company has a policy against non-OSI licenses

BgRaster's license is MPL-2.0, which is OSI-approved. The fee is a EULA on the official builds, not a term of the license, and the EULA itself defers to the MPL wherever they conflict. If your policy still objects to the EULA, [build from source](#building-from-source-instead): no EULA applies to that.

### Can I turn off the console notice?

Yes. It appears once per Windows user, after BgRaster finishes a run, and only when its output goes to an interactive console. It does not appear for `--help` or `--version`, or when output is redirected to a file or another program. Nothing checks whether anyone saw it. Once it has been shown, BgRaster creates a marker file, `%LocalAppData%\BgRaster\maintenance-fee-notice`, and does not show the notice to that user again.

To suppress it across a fleet, create that file for each user before BgRaster first runs. Its content does not matter. The login script that runs BgRaster is a good place to do it:

```bat
if not exist "%LocalAppData%\BgRaster" mkdir "%LocalAppData%\BgRaster"
type nul >> "%LocalAppData%\BgRaster\maintenance-fee-notice"
```
