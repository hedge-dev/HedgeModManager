#!/bin/bash

# Check if version argument is provided
if [ $# -eq 0 ]; then
  echo "Error: Please provide a bundle id and version number as arguments."
  echo "Usage: $0 <bundle_id> <version_number>"
  exit 1
fi


BUNDLE_ID="$1"
VERSION="$2"

APP_NAME="../output/osx-arm64/HedgeModManager.app"
PUBLISH_OUTPUT_DIRECTORY="../output/osx-arm64/."

ICON_FILE="../Source/HedgeModManager.UI/Assets/AppIcon.icns"

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir "$APP_NAME"

mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"

# Create Info.plist inside the bundle
cat > "$APP_NAME/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
    <dict>
        <key>CFBundleIconFile</key>
        <string>AppIcon.icns</string>
        <key>CFBundleIdentifier</key>
        <string>${BUNDLE_ID}</string>
        <key>CFBundleName</key>
        <string>HedgeModManager</string>
        <key>CFBundleVersion</key>
        <string>${VERSION}</string>
        <key>LSMinimumSystemVersion</key>
        <string>12.0</string>
        <key>CFBundleExecutable</key>
        <string>HedgeModManager.UI</string>
        <key>CFBundleInfoDictionaryVersion</key>
        <string>6.0</string>
        <key>CFBundlePackageType</key>
        <string>APPL</string>
        <key>CFBundleShortVersionString</key>
        <string>$1</string>
        <key>NSHighResolutionCapable</key>
        <true/>
    </dict>
</plist>
EOF

cp "$ICON_FILE" "$APP_NAME/Contents/Resources/AppIcon.icns"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_NAME/Contents/MacOS"

echo "Info.plist created successfully with CFBundleVersion: $VERSION"