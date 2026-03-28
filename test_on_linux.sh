#!/bin/bash
set -e

# ============================================
# Test script: WeasyPrint
# ============================================

CONTAINER_NAME="weasyprint-test"
IMAGE="ubuntu:22.04"
LOCAL_BINARY="./weasyprint-linux"
TEST_SCRIPT="weasyprint_test_runner.sh"

# Checks if file exists
if [ ! -f "$LOCAL_BINARY" ]; then
    echo "❌ Error: File '$LOCAL_BINARY' does not exists!"
    exit 1
fi

echo "🚀 Start test in clean Ubuntu container..."
echo ""

# Generate test script
cat > "$TEST_SCRIPT" << 'SCRIPT_EOF'
#!/bin/bash
set -e

echo '>>> Step 1: Create Test HTML...'
cat > test_input.html << 'HTML_EOF'
<!DOCTYPE html>
<html>
<head><title>Test</title></head>
<body>
    <h1>Test</h1>
    <p>Time: PLACEHOLDER_DATE</p>
</body>
</html>
HTML_EOF

sed -i "s/PLACEHOLDER_DATE/$(date)/" test_input.html

echo '>>> Step 2: Prepare binary...'
chmod +x ./weasyprint-linux

echo '>>> Step 3: Execute...'
echo '   -> Install dependencies...'
apt-get update -qq
apt-get install -y -qq \
	libpango-1.0-0 \
	libpangocairo-1.0-0 \
	libgdk-pixbuf2.0-0

if ./weasyprint-linux test_input.html test_output.pdf 2>&1; then
    echo ''
    echo '✅ Success: PDF was created.'
    if [ -f test_output.pdf ]; then
        SIZE=$(stat -c%s test_output.pdf 2>/dev/null || echo "unknown")
        echo "   File size: $SIZE bytes"
    fi
    exit 0
else
    echo ''
    echo '❌ Error: Execution failed.'
    exit 1
fi
SCRIPT_EOF

chmod +x "$TEST_SCRIPT"

docker rm -f "$CONTAINER_NAME" 2>/dev/null || true

if docker run --rm --name "$CONTAINER_NAME" \
    -v "$(pwd)":/workspace \
    -w /workspace \
    "$IMAGE" \
    bash /workspace/weasyprint_test_runner.sh; then

    echo ""
    echo "🎉 Success!"
    rm -f "$TEST_SCRIPT"
    exit 0
else
    echo ""
    echo "⚠️  Failed."
    echo ""
fi

# Aufräumen
rm -f "$TEST_SCRIPT"
echo ""
echo "✅ Cleanup done."
