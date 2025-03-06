# Building from source

## 1. Clone the repository
```bash
git clone https://github.com/hedge-dev/HedgeModManager.git
```

## 2. Build the project

###  Macos

1. Navigate to HedgeModManager.UI
2. Run the build command
```bash
dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-arm64 -p:UseAppHost=true -property:Configuration=Release -p:PublishSingleFile=true
```
3. Navigate to /bin/Release/net8.0/osx-arm64/publish
4. Start HedgeModManager.app
