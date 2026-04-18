# Copilot Instructions for `berthertogen/weasyprint.wrapped`

## Repository purpose
- This repository packages a .NET wrapper around WeasyPrint.
- Main library: `/home/runner/work/weasyprint.wrapped/weasyprint.wrapped/src/Weasyprint.Wrapped`
- Tests: `/home/runner/work/weasyprint.wrapped/weasyprint.wrapped/src/Weasyprint.Wrapped.Tests`
- Consumer samples: `Weasyprint.Wrapped.Example` and `Weasyprint.Wrapped.ExampleApi`

## Key architecture and flow
- `Printer` is the core API (`Printer.cs`):
  - `Initialize()` extracts OS-specific standalone WeasyPrint assets from zip into a working folder.
  - `Print(...)` and `PrintStream(...)` execute the extracted binary using `CliWrap`.
  - `Version()` calls `--info` on the binary.
- `ConfigurationProvider` resolves:
  - Asset zip location (`standalone-windows-64.zip` or `standalone-linux-64.zip`)
  - Working folder location (default `weasyprinter` under app base directory)
  - Base URL used by print commands.
- Tests are integration-style and depend on real standalone assets being present.

## Build, test, and validation commands
Run from repository root:

```bash
dotnet build --verbosity normal /p:UseSharedCompilation=false ./src/Weasyprint.Wrapped/Weasyprint.Wrapped.csproj
```

```bash
dotnet test --verbosity normal ./src/Weasyprint.Wrapped.Tests/Weasyprint.Wrapped.Tests.csproj
```

## Critical prerequisite for tests
- Tests require asset zips to exist at repository root under `assets/` (for Linux) and/or `assets-windows/` depending on platform and scenario.
- On Linux, failing to provide `assets/standalone-linux-64.zip` causes all integration tests to fail in `Printer.Initialize()`.

### Error encountered during onboarding
Observed while running tests in a fresh clone:

- `System.IO.DirectoryNotFoundException: Could not find a part of the path '/home/runner/work/weasyprint.wrapped/weasyprint.wrapped/assets/standalone-linux-64.zip'`
- `Test Run Failed. Total tests: 12, Failed: 12`

### Work-around
- Generate Linux asset zip before testing:

```bash
cd /home/runner/work/weasyprint.wrapped/weasyprint.wrapped
./build-on-linux.sh
```

- For Windows asset generation (when needed on Windows runners):

```powershell
cd /home/runner/work/weasyprint.wrapped/weasyprint.wrapped
./build-on-windows.ps1
```

- After assets exist, re-run `dotnet test`.

## CI expectations
- Main CI workflow: `.github/workflows/build-test-code-scan.yml`
  - CodeQL analysis job manually builds `src/Weasyprint.Wrapped`.
  - Asset build jobs create Linux and Windows standalone zip artifacts.
  - Test job downloads those artifacts and runs tests on Ubuntu and Windows.
- If tests fail locally due to missing assets but pass in CI, verify whether CI artifact-download steps are being mirrored locally.

## Practical guidance for future agent changes
- For library behavior changes, inspect and update:
  - `src/Weasyprint.Wrapped/Printer.cs`
  - `src/Weasyprint.Wrapped/Configuration/ConfigurationProvider.cs`
  - `src/Weasyprint.Wrapped.Tests/Tests/PrinterTests.cs`
- Preserve cross-platform behavior (`RuntimeInformation.IsOSPlatform`) and asset naming conventions (`standalone-{env}-64.zip`).
- When modifying command execution in `Printer`, keep `CommandResultValidation.None` and stderr filtering behavior in mind (`GLib-GIO-WARNING` is intentionally ignored).
- Avoid changing asset folder conventions unless tests and packaging content entries in `Weasyprint.Wrapped.csproj` are updated consistently.

## Packaging and release notes
- NuGet packing is driven from `src/Weasyprint.Wrapped/Weasyprint.Wrapped.csproj` and release workflow.
- Package content includes assets from root-level `assets/`, `assets-linux/`, and `assets-windows/` folders.
- Tag-based release workflow (`release-assets.yml`) creates release assets and pushes packages/images.
