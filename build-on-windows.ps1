$workingDir = "./standalone-windows-64";
$assets = "./assets";
$version = "weasyprint==68.1"

if (Test-Path $workingDir) {
    Write-Host "*** Cleaning $workingDir"
    Remove-Item $workingDir -Recurse -Force | Out-Null
}
Write-Host "*** Creating $workingDir"
New-Item -ItemType "directory" -Path $workingDir | Out-Null

if (!(Test-Path $assets)) {
    Write-Host "*** Creating $assets"
    New-Item -ItemType "directory" -Path $assets | Out-Null
}

$workingDir = Resolve-Path -Path "./standalone-windows-64";
$assets = Resolve-Path -Path "./assets";

Write-Host "*** Downloading msys2 environment"
Invoke-WebRequest -Uri "https://github.com/msys2/msys2-installer/releases/download/2025-12-13/msys2-base-x86_64-20251213.sfx.exe" -OutFile "$workingDir/msys2-base-x86_64-20251213.sfx.exe"
Write-Host "*** Extracting msys2 environment"
Invoke-Expression "$workingDir/msys2-base-x86_64-20251213.sfx.exe -y -o$workingDir"
Remove-Item "$workingDir/msys2-base-x86_64-20251213.sfx.exe" -Recurse -Force | Out-Null

Write-Host "*** Downloading python (https://github.com/indygreg/python-build-standalone)"
Invoke-WebRequest -Uri "https://github.com/astral-sh/python-build-standalone/releases/download/20260408/cpython-3.13.13+20260408-x86_64-pc-windows-msvc-install_only.tar.gz" -OutFile "$workingDir/python.tar.gz"
Write-Host "*** Extracting python"
Invoke-Expression "tar -xvzf $workingDir/python.tar.gz -C $workingDir"
Remove-Item "$workingDir/python.tar.gz" -Recurse -Force | Out-Null

Write-Host "*** Installing weasyprint dependencies"
Invoke-Expression "$workingDir\msys64\usr\bin\bash -lc 'pacman -S mingw-w64-x86_64-pango mingw-w64-x86_64-sed --noconfirm'"

Write-Host "*** Remove python.exe to avoid conflicts with msys2 python"
Remove-Item "$workingDir\msys64\mingw64\bin\python.exe"

Write-Host "*** Installing weasyprint and pyinstaller"
Invoke-Expression "$workingDir\python\python.exe -m pip install weasyprint==68.1 pyinstaller"

Write-Host "*** Patching weasyprint __main__.py to fix imports"
Invoke-Expression "$workingDir\msys64\mingw64\bin\sed -i 's/^from \. /from weasyprint /' $workingDir/python/Lib/site-packages/weasyprint/__main__.py"
Invoke-Expression "$workingDir\msys64\mingw64\bin\sed  -i 's/^from \./from weasyprint\./' $workingDir/python/Lib/site-packages/weasyprint/__main__.py"
$Env:PATH = "$workingDir\msys64\mingw64\bin;$Env:PATH"

Write-Host "*** Building weasyprint executable"
Invoke-Expression "$workingDir\python\python.exe -m PyInstaller $workingDir/python/Lib/site-packages/weasyprint/__main__.py -n weasyprint -D"

Write-Host "*** Cleaning up python and msys2 environments for testing weasyprint executable"
Remove-Item "$workingDir/python" -Recurse -Force | Out-Null
Remove-Item "$workingDir/msys64" -Recurse -Force | Out-Null

Set-Location  "./dist/"
Move-Item -Path "./weasyprint" -Destination "./weasyprint-windows"
New-Item -Path "version-$version"

Set-Location  "./weasyprint-windows"
Write-Host "*** Testing weasyprint"
Invoke-Expression ".\weasyprint.exe --info"

Set-Location  "../"
Write-Host "*** Create archive $assets/standalone-windows-64.zip"
Compress-Archive -Path "./*"  -DestinationPath "$assets/standalone-windows-64.zip" -CompressionLevel "Optimal" -Force

Set-Location  "../"
Write-Host "*** Clean up dist and build directories"
Remove-Item -Path "./dist" -Recurse -Force | Out-Null
Remove-Item -Path "./build" -Recurse -Force | Out-Null