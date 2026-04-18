# AGENTS.md

## Purpose
- `Weasyprint.Wrapped` is a .NET wrapper that bundles a standalone `weasyprint` CLI (zip asset) so consumers do not install Python/system deps manually (`readme.md`, `src/Weasyprint.Wrapped/Printer.cs`).
- Runtime behavior is OS-specific: Windows runs `weasyprint.exe`, Linux runs `weasyprint` binary extracted from platform zip (`src/Weasyprint.Wrapped/Printer.cs`, `src/Weasyprint.Wrapped/Configuration/ConfigurationProvider.cs`).

## Repo map (what matters first)
- Library: `src/Weasyprint.Wrapped/` (`Printer`, config, result DTOs, init exception).
- Integration tests: `src/Weasyprint.Wrapped.Tests/Tests/PrinterTests.cs` (best source of expected behavior).
- Usage samples: `src/Weasyprint.Wrapped.Example/Program.cs` and `src/Weasyprint.Wrapped.ExampleApi/Controllers/PrintController.cs`.
- Asset builders: `build-on-windows.ps1`, `build-on-linux.sh`, `test_on_linux.sh`.
- CI/release behavior: `.github/workflows/build-test-code-scan.yml`, `.github/workflows/release-assets.yml`.

## Core runtime flow
- Call `await printer.Initialize()` before print/version calls; it extracts `assets/standalone-{windows|linux}-64.zip` to working folder only when `version-*` marker changes.
- Version pinning is file-based: zip must contain `version-*`; `Initialize()` skips re-extract when matching marker exists (`Printer.Initialize`).
- Print from HTML string uses stdin/stdout (`- -`) and returns bytes via `PrintResult.Bytes`; stream variant returns open stream (`PrintStreamResult.DocumentStream`).
- Print from file path writes directly to output file and returns metadata only (`Print(string htmlFile, string pdfFile, ...)`).
- Known noisy stderr is filtered (`GLib-GIO-WARNING`) via `IgnoreCertainErrors`; do not treat that warning as functional failure.

## Developer workflows
- Build library/package locally from repo root:
  - `dotnet build src/Weasyprint.Wrapped/Weasyprint.Wrapped.csproj`
  - `dotnet pack src/Weasyprint.Wrapped/Weasyprint.Wrapped.csproj -c Release --output src/Weasyprint.Wrapped/nupkgs`
- Run tests (requires platform asset zip present under `assets/`):
  - `dotnet test src/Weasyprint.Wrapped.Tests/Weasyprint.Wrapped.Tests.csproj`
- Rebuild bundled assets:
  - Windows: `./build-on-windows.ps1`
  - Linux: `./build-on-linux.sh` (Docker required), optional smoke test `./test_on_linux.sh`.

## Project-specific conventions (follow these)
- `ConfigurationProvider` defaults to `AppContext.BaseDirectory`; tests override with relative asset path (`../../../../../assets/`) and working folder `weasyprinter`.
- Additional CLI args are passed through as raw strings (example: `"--optimize-images"` in tests and example app).
- Cross-platform behavior is validated by selecting expected files per OS (`PrinterTests` chooses Windows vs Linux expected PDF).
- `src/Weasyprint.Wrapped/Weasyprint.Wrapped.csproj` packs assets into `contentFiles/any/any`; changing asset names/paths requires updating both packaging and runtime lookup.
- `nuget.config` includes local source `./src/Weasyprint.Wrapped/nupkgs/` for iterative testing with sample projects.

## Integration and release points
- CI builds assets on both OS runners, uploads artifacts, then runs tests against downloaded artifacts (`build-test-code-scan.yml`).
- Linux CI sets isolated font config env vars before tests; font issues in CI are often environment-related, not wrapper logic.
- Tag-based release (`v*`) creates draft prerelease, rewrites `version-*` files inside zips, packs NuGet, pushes to GitHub/NuGet, and publishes zip assets (`release-assets.yml`).
- Docker image `.docker/net-sdk-weayprint/Dockerfile` is published in release workflow and provides runtime deps for WeasyPrint scenarios.

