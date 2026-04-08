#!/bin/bash
set -e  # End the script at errors

# ============================================
# WeasyPrint Portable Build Script
# ============================================

BUILD_DIR="weasyprint-build"
OUTPUT_NAME="weasyprint-linux"
DOCKER_IMAGE="ubuntu:22.04"
LOG_FILE="build.log"
ASSETS_DIR="assets"
VERSION="weasyprint==68.1"

echo "🔧 WeasyPrint Portable Builder (Debug)"
echo "======================================"
echo ""

# 1. Prepare build and assets directory
echo "[1/7] Prepare directories..."
if [ -d "$BUILD_DIR" ]; then
    echo "   ⚠️  Existing directory will be deleted..."
    rm -rf "$BUILD_DIR"
fi
mkdir -p "$BUILD_DIR"

if [ -d "$ASSETS_DIR" ]; then
    echo "   ⚠️  Existing directory will be deleted..."
    rm -rf "$ASSETS_DIR"
fi
mkdir -p "$ASSETS_DIR"

cd "$BUILD_DIR"

# 2. Create wrapper script
echo "[2/7] Create wrapper script..."
cat > run_weasyprint.py << 'EOF'
#!/usr/bin/env python3
import sys
from weasyprint.__main__ import main

if __name__ == "__main__":
    main()
EOF

# 3. Start docker container
echo "[3/7] Start docker container..."
echo "   Please wait... (could take some time)"

# docker command with output and error handling
docker run --rm \
    -v "$(pwd)":/build \
    -w /build \
    -e DEBIAN_FRONTEND=noninteractive \
    "$DOCKER_IMAGE" \
    bash -c '
    set -e
    
    echo ">>> Step 1: System update..."
    apt-get update
    
    echo ">>> Step 2: Install system dependencies..."
    apt-get install -y \
        python3 \
        python3-pip \
        python3-venv \
        build-essential \
        libpango-1.0-0 \
        libpangocairo-1.0-0 \
        libgdk-pixbuf2.0-0 \
        libffi-dev \
        shared-mime-info \
        git \
        ca-certificates
    
    echo ">>> Step 3: Create virt environment..."
    python3 -m venv venv
    source venv/bin/activate
    
    echo ">>> Step 4: Update pip..."
    pip install --upgrade pip
    
    echo ">>> Step 5: Install WeasyPrint und PyInstaller..."
    pip install '"$VERSION"' pyinstaller
    
    echo ">>> Step 6: Check and adjust imports..."
    WEASYPRINT_MAIN=$(find venv -name "__main__.py" -path "*/weasyprint/*" | head -1)
    if [ -f "$WEASYPRINT_MAIN" ]; then
        echo "   Found: $WEASYPRINT_MAIN"
        sed -i "s/^from \. /from weasyprint /" "$WEASYPRINT_MAIN"
        sed  -i "s/^from \./from weasyprint\./" "$WEASYPRINT_MAIN"
    else
        echo "   WARNING: __main__.py not found!"
    fi
    
    echo ">>> Step 7: Build PyInstaller..."
    pyinstaller --onedir --name weasyprint run_weasyprint.py
    
    echo ">>> Step 8: Check result..."
    if [ -d "dist/weasyprint" ]; then
        ls -lh dist/weasyprint
        cp -rv dist/weasyprint /build/'"$OUTPUT_NAME"'
        echo ">>> SUCCESS: File was created!"
    else
        echo ">>> ERROR: dist/weasyprint does not exists!"
        exit 1
    fi
    '

# 4. Check if file exists
echo "[4/7] Check if file exists..."
if [ ! -d "$OUTPUT_NAME" ]; then
    echo "❌ ERROR: The file $OUTPUT_NAME was not created!"
    echo ""
    echo "Possible reasons:"
    echo "  1. Docker run was not correct"
    echo "  2. apt-get or pip install failed"
    echo "  3. PyInstaller build failed"
    echo ""
    echo "Check the docker output above for details."
    exit 1
fi

# 5. Short test
echo "[5/7] Output version (inside Docker)..."
if docker run --rm \
    -e OUTPUT_NAME="$OUTPUT_NAME" \
    -v "$PWD":/build \
    -w /build \
    "$DOCKER_IMAGE" \
    /bin/bash -lc 'apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends libpango-1.0-0 libpangocairo-1.0-0 libgdk-pixbuf-2.0-0 >/dev/null 2>&1 && "./$OUTPUT_NAME/weasyprint" --info'; then
    echo "✅ Version test passed inside Docker!"
else
    echo "⚠️ Version test failed (maybe because of missing system libraries in Docker image)"
    exit 1
fi

echo ""
echo "============================================"
echo "✅ Build finished!"
echo "   Size: $(du -h "$OUTPUT_NAME" | cut -f1)"
echo ""
echo "📝 Usage:"
echo "   ./$OUTPUT_NAME input.html output.pdf"
echo "============================================"

cp -r $OUTPUT_NAME ../

echo "[6/7] Creating version file..."
cd ..
touch "version-$VERSION"

echo "[7/7] Creating archive $ASSETS_DIR/standalone-linux-64.zip..."
zip -r "./$ASSETS_DIR/standalone-linux-64.zip" "version-$VERSION" "$OUTPUT_NAME"

# Cleanup
echo "Cleanup..."
rm -r "$OUTPUT_NAME" "version-$VERSION"

