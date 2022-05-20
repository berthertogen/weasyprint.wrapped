# Warning

This is a large package, try to limit the projects where it will be installed.
+-200mb zipped resource (standalone python, gtk3 for windows and weasyprint) which will be unzipped on initialization in the same folder.

# Usage

You might want to provide the printer class using your DI Container.

1. Initialize
This will unzip the needed asset, this is best done during startup procedure.
The printer will check if the asset is already unzipped, so the process is only done once.
```csharp
new Printer().Initialize();
```
2. Print
```csharp
new Printer().Print("<html><body><h1>TEST</h1></body></html>");
```

# Create test package and run example

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
dotnet pack -p:PackageVersion=0.0.[[NUMBER HERE]] --output nupkgs
```

Update the example project package version (Weasyprint.Wrapped.Example.csproj) and run the example to test it

# Building assets

## Windows (build-on-windows.ps1 does approximately this)

* Download https://github.com/tschoonj/GTK-for-Windows-Runtime-Environment-Installer/releases and install to C:\weasyprint.wrapped\standalone-win-64\gtk
* Download https://github.com/indygreg/python-build-standalone/releases (correct release, eg: cpython-3.10.4+20220502-x86_64-pc-windows-msvc-shared-install_only.tar.gz for windows) and extract to c:\weasyprint.wrapped\standalone-win-64\python
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

## Linux (build-on-linux.ps1 does approximately this)

* Install powershell on ubuntu (https://docs.microsoft.com/en-us/powershell/scripting/install/install-ubuntu?view=powershell-7.2)
* Download https://github.com/indygreg/python-build-standalone/releases (correct release, eg: cpython-3.10.4+20220502-x86_64-unknown-linux-gnu-install_only.tar.gz for Linux) and extract to c:\weasyprint.wrapped\standalone-linux-64\python
* Install weasyprint in the standalone python
```
cd c:\weasyprint.wrapped\standalone-linux-64\python\bin\
python3 -m pip install weasyprint
python3 -m weasyprint --info
```

## Help

* Check PATH in python
```
.\python.exe -c "import os; print(*os.environ['PATH'].split(os.pathsep), sep='\n')"
```

# Externals

* https://github.com/balbarak/WeasyPrint-netcore
* https://github.com/Kozea/WeasyPrint
* https://wiki.python.org/moin/EmbeddedPython
* https://github.com/tschoonj/GTK-for-Windows-Runtime-Environment-Installer/releases