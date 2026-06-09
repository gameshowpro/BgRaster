# Feature Gallery

This document is intentionally structured as simple sections so automated tooling can refresh image links and keep descriptions current.

Generated PNGs are expected in `docs/generated` and should be regenerated with `scripts/generate-documentation.ps1`.

## Background Color

- What it demonstrates: solid background color fill.
- Sample config: [background.toml](sample-config/background.toml)

<details>
<summary>View sample TOML</summary>

```toml
--8<-- "sample-config/background.toml"
```
</details>

![Background sample](generated/background.png)

## Background Image

- What it demonstrates: background bitmap rendering and fit behavior.
- Sample config: [background-image.toml](sample-config/background-image.toml)

<details>
<summary>View sample TOML</summary>

```toml
--8<-- "sample-config/background-image.toml"
```
</details>

![Background image sample](generated/background-image.png)

## Background Alternating

- What it demonstrates: alternating-pixel checker pattern used for signal verification. You probably need to zoom in to appreciate this. However you try to zoom it, it will probably look mushy. It's already telling you something about your render pipeline!
- Sample config: [background-alternating.toml](sample-config/background-alternating.toml)

<details>
<summary>View sample TOML</summary>

```toml
--8<-- "sample-config/background-alternating.toml"
```
</details>

![Background alternating sample](generated/background-alternating.png)

## Grid

- What it demonstrates: checker/grid layer with labels for column and row numbers. The labels include an triangle to mark the corner that the text refers to.
- Sample config: [grid.toml](sample-config/grid.toml)

<details>
<summary>View sample TOML</summary>

```toml
--8<-- "sample-config/grid.toml"
```
</details>

![Grid sample](generated/grid.png)

## Circle

- What it demonstrates: centered circle sizing and stroke behavior.
- Sample config: [circle.toml](sample-config/circle.toml)

<details>
<summary>View sample TOML</summary>

```toml
--8<-- "sample-config/circle.toml"
```
</details>

![Circle sample](generated/circle.png)

## Crosshair

- What it demonstrates: centered crosshair with configurable arm length and stroke.
- Sample config: [crosshair.toml](sample-config/crosshair.toml)

<details>
<summary>View sample TOML</summary>

```toml
--8<-- "sample-config/crosshair.toml"
```
</details>

![Crosshair sample](generated/crosshair.png)

## Labeled Edges

- What it demonstrates: edge labels, tail-length, and scope behavior.
- Sample config: [labeled-edges.toml](sample-config/labeled-edges.toml)

<details>
<summary>View sample TOML</summary>

```toml
--8<-- "sample-config/labeled-edges.toml"
```
</details>

![Labeled edges sample](generated/labeled-edges.png)

## Text + Logo

- What it demonstrates: text composition and inclusion of arbitrary logo bitmap.
- Sample config: [text-logo.toml](sample-config/text-logo.toml)

<details>
<summary>View sample TOML</summary>

```toml
--8<-- "sample-config/text-logo.toml"
```
</details>

![Text and logo sample](generated/text-logo.png)

## Slices

- What it demonstrates: output split into four custom-sized slices, with varying options. Demonstrates many features in a single example.
- Sample config: [slices.toml](sample-config/slices.toml)

<details>
<summary>View sample TOML</summary>

```toml
--8<-- "sample-config/slices.toml"
```
</details>

![Slices sample](generated/slices.png)