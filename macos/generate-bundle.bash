#!/bin/bash
VERSION=${1:-"1.0.0"}

APP_NAME="../output/macos/HedgeModManager.app"
PLIST_PATH="./Info.plist" 
PUBLISH_OUTPUT_DIRECTORY="../output/macos/osx-arm64/."

ICON_FILE="./AppIcon.icns"

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir "$APP_NAME"

mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"

cp "$ICON_FILE" "$APP_NAME/Contents/Resources/AppIcon.icns"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_NAME/Contents/MacOS"
cp -a "$PLIST_PATH" "$APP_NAME/Contents/Info.plist"

PLIST_PATH="$APP_NAME/Contents/Info.plist"
  
# Update CFBundleShortVersionString using sed
sed -i '' -e "s/<key>CFBundleShortVersionString<\/key>\s*<string>[^<]*<\/string>/<key>CFBundleShortVersionString<\/key><string>${VERSION}<\/string>/g" "$PLIST_PATH"
        
# Update CFBundleVersion using sed
sed -i '' -e "s/<key>CFBundleVersion<\/key>\s*<string>[^<]*<\/string>/<key>CFBundleVersion<\/key><string>${VERSION}<\/string>/g" "$PLIST_PATH"
