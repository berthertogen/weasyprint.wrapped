![build-test-code-scan](https://github.com/berthertogen/weasyprint.wrapped/actions/workflows/build-test-code-scan.yml/badge.svg)

# Introduction

Wrapper around the [weasyprint](https://github.com/Kozea/WeasyPrint) library which allows you to print html to a pdf file.
This package does not require any external software or packages to be installed in order to use weasyprint.

# Getting started

## Installation

[![NuGet Badge](https://buildstats.info/nuget/Weasyprint.Wrapped)](https://www.nuget.org/packages/Weasyprint.Wrapped/)

```shell
dotnet add package Weasyprint.Wrapped
```

## Usage

You might want to provide the printer class using your DI Container.

1. Initialize  
   This will unzip the needed asset, it is best done during startup procedure.
   The printer will check if the asset is already unzipped, so the process is only done once (or when the package is upgraded and the version changed).

```csharp
await new Printer().Initialize();
```

2. Print

```csharp
await new Printer().Print("<html><body><h1>TEST</h1></body></html>");
```

## Warning

This is a large package, try to limit the projects where it will be installed.

- +-104 MB zipped resource (standalone python, gtk3 for windows and weasyprint) which will be unzipped on initialization in the same folder (for windows).
- +-89.7 MB zipped resource (standalone python and weasyprint) which will be unzipped on initialization in the same folder (for linux).

# Extra resources

## References

Thanks to all these great repos and the guys maintaining them!!!

* https://github.com/balbarak/WeasyPrint-netcore
* https://github.com/Kozea/WeasyPrint
* https://wiki.python.org/moin/EmbeddedPython
* https://github.com/tschoonj/GTK-for-Windows-Runtime-Environment-Installer/releases

## [Weasyprint CLI](https://doc.courtbouillon.org/weasyprint/stable/api_reference.html#command-line-api)

The weasyprint program takes at least two arguments:

`weasyprint [options] <input> <output>`

* `input`: URL or filename of the HTML input, or - for stdin.

* `output`: Filename where output is written, or - for stdout.

* `-e <encoding>, --encoding <encoding>`: Force the input character encoding.

* `-s <stylesheet>, --stylesheet <stylesheet>` URL or filename for a user CSS stylesheet.

This option can be passed multiple times.

* `-m <media-type>, --media-type <media-type>` Media type to use for @media, defaults to print.

* `-u <base-url>, --base-url <base-url>` Base for relative URLs in the HTML input, defaults to the input’s own filename or URL or the current directory for stdin.

* `-a <attachment>, --attachment <attachment>` URL or filename of a file to attach to the PDF document. This option can be passed multiple times.

* `--pdf-identifier <pdf-identifier>` PDF file identifier.

* `--pdf-variant <pdf-variant>` PDF variant to generate.

  Possible choices: pdf/a-1b, pdf/a-2b, pdf/a-3b, pdf/a-4b, pdf/ua-1.

* `--pdf-version <pdf-version>` PDF version number.

* `--pdf-forms` Include PDF forms.

* `--uncompressed-pdf` Do not compress PDF content, mainly for debugging purpose.

* `--custom-metadata` Include custom HTML meta tags in PDF metadata.

* `-p, --presentational-hints` Follow HTML presentational hints.

* `--optimize-images` Optimize size of embedded images with no quality loss.

* `-j <jpeg-quality>, --jpeg-quality <jpeg-quality>` JPEG quality between 0 (worst) to 95 (best).

* `--full-fonts` Embed unmodified font files when possible.

* `--hinting` Keep hinting information in embedded fonts.

* `-c <cache-folder>, --cache-folder <cache-folder>` Store cache on disk instead of memory, folder is created if needed and cleaned after the PDF is generated.

* `-D <dpi>, --dpi <dpi>` Set maximum resolution of images embedded in the PDF.

* `-v, --verbose` Show warnings and information messages.

* `-d, --debug` Show debugging messages.

* `-q, --quiet` Hide logging messages.

* `--version` Print WeasyPrint’s version number and exit.

* `-i, --info` Print system information and exit.

* `-O <optimize-size>, --optimize-size <optimize-size>` Deprecated, use other options instead.

  Possible choices: images, fonts, hinting, pdf, all, none.
  This option can be passed multiple times.

* `-h, --help`: Show this help message and exit.

# Contribute

## Create test package and run example

Windows:

```
.\build-on-windows.ps1
```

Linux:

```
.\build-on-linux.ps1
```

Create package:

```
cd .\src\Weasyprint.Wrapped\
dotnet pack -p:PackageVersion=0.0.25 --output nupkgs
```

Update the example project package version (Weasyprint.Wrapped.Example.csproj) and run the example to test it

## Error in tests

The following error might be thrown when running the tests on windows:

``` shell
(process:13448): GLib-GIO-WARNING **: 17:21:45.816: Unexpectedly, UWP app `Microsoft.OutlookForWindows_1.2023.1114.100_x64__8wekyb3d8bbwe' (AUMId `Microsoft.OutlookForWindows_8wekyb3d8bbwe!Microsoft.OutlookforWindows') supports 1 extensions but has no verbs
```

Check here for the answer: <https://stackoverflow.com/questions/67607643/what-does-this-warning-mean-it-happens-every-time-i-restart-the-node-process#answer-67715630>

## Building assets

### Windows (build-on-windows.ps1 does approximately this)

* Download https://github.com/tschoonj/GTK-for-Windows-Runtime-Environment-Installer/releases and install to C:\weasyprint.wrapped\standalone-win-64\gtk
* Download https://github.com/indygreg/python-build-standalone/releases (correct release, eg: cpython-3.10.4+20220502-x86_64-pc-windows-msvc-shared-install_only.tar.gz for windows) and extract to c:
  \weasyprint.wrapped\standalone-win-64\python
* Add gtk3 to path (in current session)

``` powershell
$Env:PATH += ";C:\weasyprint.wrapped\standalone-win-64\gtk3\bin"
```

* Install weasyprint in the standalone python

```
cd c:\weasyprint.wrapped\standalone-win-64\python
.\python.exe -m pip install weasyprint
.\python.exe -m weasyprint --info
```

### Linux (build-on-linux.ps1 does approximately this)

* Install powershell on ubuntu (https://docs.microsoft.com/en-us/powershell/scripting/install/install-ubuntu?view=powershell-7.2)
* Download https://github.com/indygreg/python-build-standalone/releases (correct release, eg: cpython-3.10.4+20220502-x86_64-unknown-linux-gnu-install_only.tar.gz for Linux) and extract to c:
  \weasyprint.wrapped\standalone-linux-64\python
* Install weasyprint in the standalone python

```
cd c:\weasyprint.wrapped\standalone-linux-64\python\bin\
python3 -m pip install weasyprint
python3 -m weasyprint --info
```

### Help

* Check PATH in python

```
.\python.exe -c "import os; print(*os.environ['PATH'].split(os.pathsep), sep='\n')"
```