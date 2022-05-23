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
Invoke-Expression "./python3 -m venv venv"
Invoke-Expression "/bin/bash source $workingDir/python/bin/venv/bin/activate"
Invoke-Expression "$workingDir/python/bin/venv/bin/pip install weasyprint"
Invoke-Expression "$workingDir/python/bin/venv/bin/weasyprint --info"
$version = Invoke-Expression "$workingDir/python/bin/venv/bin/weasyprint --version"
$versionCleared = $version.Replace(' ','').ToLower();
$versionFile = "version-$versionCleared"
New-Item -Path "$workingDir/$versionFile"
New-Item -Path "$workingDir/print.sh"
Add-Content -Path "$workingDir/print.sh" -Value "
chmod -R 777 ./python
source ./python/bin/venv/bin/activate
./python/bin/venv/bin/weasyprint - - --encoding utf8
"
Invoke-Expression "/bin/bash sed -i -e 's/\r$//' $workingDir/print.sh"
Invoke-Expression "chmod -R 777 $workingDir"
Set-Location  "../../../"

Write-Host "*** Create archive $assets/standalone-linux-64.zip"
Compress-Archive -Path "$workingDir/*"  -DestinationPath "$assets/standalone-linux-64.zip" -CompressionLevel "Optimal" -Force