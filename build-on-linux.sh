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

echo "*** Downloading python (https://github.com/indygreg/python-build-standalone)"
wget -O $workingDir/python.tar.gz 'https://github.com/indygreg/python-build-standalone/releases/download/20220502/cpython-3.10.4+20220502-x86_64-unknown-linux-gnu-install_only.tar.gz'
tar -xvzf $workingDir/python.tar.gz -C $workingDir
rm $workingDir/python.tar.gz

cd $workingDir

versionClearedLowered=weasyprint==55
echo "*** Version=$versionClearedLowered"
touch "version-$versionClearedLowered"
echo "cd python/bin/
python3 -m pip install $versionClearedLowered" >> init.sh

echo "*** Create archive $assets/standalone-linux-64.zip"
zip -9 -y -r "../$assets/standalone-linux-64.zip" .