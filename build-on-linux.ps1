$workingDir = "./standalone-linux-64";
$assets = "./assets";

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

$workingDir = Resolve-Path -Path "./standalone-linux-64";
$assets = Resolve-Path -Path "./assets";

Write-Host "*** Downloading python (https://github.com/indygreg/python-build-standalone)"
Invoke-WebRequest -Uri "https://github.com/indygreg/python-build-standalone/releases/download/20220502/cpython-3.10.4+20220502-x86_64-unknown-linux-gnu-install_only.tar.gz" -OutFile "$workingDir/python.tar.gz"
Invoke-Expression "tar -xvzf $workingDir/python.tar.gz -C $workingDir"
Remove-Item "$workingDir/python.tar.gz" -Recurse -Force | Out-Null

Set-Location  "$workingDir/python/bin"
Write-Host "*** Installing weasyprint"
Invoke-Expression "./python3 -m pip install weasyprint==55 -t ../lib/"
$Env:PYTHONPATH += ":$workingDir/python/lib"
Write-Host "*** Testing weasyprint"
Invoke-Expression "./python3 -m weasyprint --info"
$version = Invoke-Expression "./python3 -m weasyprint --version"
$versionCleared = $version.Replace(' ','').ToLower();
$versionFile = "version-$versionCleared"
New-Item -Path "$workingDir/$versionFile"
Invoke-Expression "chmod -R 777 $workingDir"
Set-Location  "../../../"

Write-Host "*** Create archive $assets/standalone-linux-64.zip"
Compress-Archive -Path "$workingDir/*"  -DestinationPath "$assets/standalone-linux-64.zip" -CompressionLevel "Optimal" -Force