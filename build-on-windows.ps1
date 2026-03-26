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

Write-Host "*** Downloading weasyprint Windows executable"
Invoke-WebRequest -Uri "https://github.com/Kozea/WeasyPrint/releases/download/v68.1/weasyprint-windows.zip" -OutFile "$workingDir/weasyprint-windows.zip"
Expand-Archive -Path "$workingDir/weasyprint-windows.zip" -DestinationPath "$workingDir" -Force
Remove-Item "$workingDir/weasyprint-windows.zip" -Recurse -Force | Out-Null

Set-Location  "$workingDir"
Move-Item -Path "$workingDir/dist/weasyprint.exe" -Destination "$workingDir/weasyprint.exe"
Remove-Item -Path "$workingDir/dist"

Write-Host "*** Testing weasyprint"
Invoke-Expression ".\weasyprint.exe --info"
New-Item -Path "$workingDir/version-$version"
Set-Location  "../"

Write-Host "*** Create archive $assets/standalone-windows-64.zip"
Compress-Archive -Path "$workingDir/*"  -DestinationPath "$assets/standalone-windows-64.zip" -CompressionLevel "Optimal" -Force