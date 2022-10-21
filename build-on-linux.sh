workingDir="./standalone-linux-64";
assets="./assets";
version=weasyprint==57

if [ -d "$workingDir" ]; then
  echo "*** Cleaning $workingDir"
  rm -rf $workingDir
fi

echo "*** Creating $workingDir"
mkdir $workingDir

if [ -d "$assets" ]; then
  echo "*** Creating $assets"
  rm -rf $assets
fi

echo "*** Creating $assets"
mkdir $assets

echo "*** Downloading python (https://github.com/indygreg/python-build-standalone)"
wget -O $workingDir/python.tar.gz 'https://github.com/indygreg/python-build-standalone/releases/download/20220502/cpython-3.10.4+20220502-x86_64-unknown-linux-gnu-install_only.tar.gz'
tar -xvzf $workingDir/python.tar.gz -C $workingDir
rm $workingDir/python.tar.gz

cd $workingDir/python/bin/
./python3.10 -m pip install $version 2> /dev/null
./python3.10 -m weasyprint --info

echo "*** Version=$version"
cd ../../
touch "version-$version"
echo "cd python/bin/
./python3.10 -m weasyprint - - --encoding utf8" >> print.sh

echo "*** Create archive $assets/standalone-linux-64.zip"
zip -r "../$assets/standalone-linux-64.zip" .