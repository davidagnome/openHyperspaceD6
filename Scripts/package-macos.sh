#!/usr/bin/env bash
# Wraps a published macOS binary into a TerminalHyperspace.app bundle so Finder
# and the Dock pick up the logo icon and bundle metadata.
#
# Usage:  Scripts/package-macos.sh <rid>
#         where <rid> is osx-arm64 or osx-x64
set -euo pipefail

RID="${1:-osx-arm64}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PUBLISH_DIR="$ROOT/bin/Release/net10.0/$RID/publish"
BIN="$PUBLISH_DIR/TerminalHyperspace"
ICON="$ROOT/Assets/logo.icns"

if [[ ! -f "$BIN" ]]; then
  echo "error: $BIN not found — run 'dotnet publish -c Release -r $RID --self-contained' first" >&2
  exit 1
fi
if [[ ! -f "$ICON" ]]; then
  echo "error: $ICON not found" >&2
  exit 1
fi

APP="$PUBLISH_DIR/TerminalHyperspace.app"
rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"

cp "$BIN" "$APP/Contents/MacOS/TerminalHyperspace"
chmod +x "$APP/Contents/MacOS/TerminalHyperspace"
cp "$ICON" "$APP/Contents/Resources/logo.icns"

cat > "$APP/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>            <string>Terminal Hyperspace</string>
    <key>CFBundleDisplayName</key>     <string>Terminal Hyperspace</string>
    <key>CFBundleIdentifier</key>      <string>com.openhyperspace.terminalhyperspace</string>
    <key>CFBundleVersion</key>         <string>1.0.0</string>
    <key>CFBundleShortVersionString</key><string>1.0.0</string>
    <key>CFBundleExecutable</key>      <string>TerminalHyperspace</string>
    <key>CFBundleIconFile</key>        <string>logo</string>
    <key>CFBundlePackageType</key>     <string>APPL</string>
    <key>LSMinimumSystemVersion</key>  <string>11.0</string>
    <key>NSHighResolutionCapable</key> <true/>
    <key>NSPrincipalClass</key>        <string>NSApplication</string>
</dict>
</plist>
PLIST

# Finder needs CFBundleIconFile to point at "logo" (no extension), so rename
mv "$APP/Contents/Resources/logo.icns" "$APP/Contents/Resources/logo.icns"

echo "Built: $APP"
