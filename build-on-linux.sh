workingDir="./standalone-linux-64";
assets="./assets";

if [ -d "$workingDir" ]; then
  echo "*** Cleaning $workingDir"
  rm -rf $workingDir
fi

echo "*** Creating $workingDir"
mkdir $workingDir

if ! [ -d "$assets" ]; then
  echo "*** Creating $assets"
  mkdir $assets
fi

# $workingDir = Resolve-Path -Path "./standalone-linux-64";
# $assets = Resolve-Path -Path "./assets";

echo "*** Downloading python (https://github.com/indygreg/python-build-standalone)"
wget -O $workingDir/python.tar.gz 'https://github.com/indygreg/python-build-standalone/releases/download/20220502/cpython-3.10.4+20220502-x86_64-unknown-linux-gnu-install_only.tar.gz'
tar -xvzf $workingDir/python.tar.gz -C $workingDir
rm $workingDir/python.tar.gz

cd $workingDir/python/
echo "*** Installing weasyprint"
bin/python3 -m venv venv
source venv/bin/activate
venv/bin/pip install weasyprint
venv/bin/weasyprint --info
version=$(venv/bin/weasyprint --version)
versionCleared="${version// /}"
versionClearedLowered=$(echo $versionCleared | tr '[:upper:]' '[:lower:]')
echo "*** Version=$versionClearedLowered"
cd ..
touch $versionClearedLowered
echo "chmod -R 777 python
cd python
source venv/bin/activate
venv/bin/weasyprint - - --encoding utf8" >> print.sh
# Invoke-Expression "sed -i -e 's/\r$//' $workingDir/print.sh"
# Invoke-Expression "chmod -R 777 $workingDir"
cd ../
ls

echo "*** Create archive $assets/standalone-linux-64.zip"
zip -9 -y -r $assets/standalone-linux-64.zip $workingDir/*