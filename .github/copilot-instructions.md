# GitHub Copilot Instructions

All code generation and edits in this repository must comply with the coding standards defined in:

- `docs/coding-standards.md`

## Enforcement rules

- Treat `docs/coding-standards.md` as authoritative for all coding style and architecture guidance.
- Follow the standards for C#, .NET, naming, async usage, nullable usage, and analyzer handling.
- Prefer modern C# 13 syntax and patterns as required by the standards.
- Preserve and extend existing code in a standards-compliant way when modifying files.
- If any requested change conflicts with `docs/coding-standards.md`, follow the standards and surface the conflict clearly.

## Scope

These instructions apply to all repository files unless a more specific, nested Copilot instructions file overrides them.
