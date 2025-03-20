# Building from source

## 1. Clone the repository
```zsh
git clone https://github.com/hedge-dev/HedgeModManager.git
```

## 2. Build the project

###  macOS

1. Navigate to the root of the project
2. Run the build command
```zsh
dotnet publish -p:PublishProfile=osx-arm64 -c Release -o ./output/macos/osx-arm64 ./Source/HedgeModManager.UI/HedgeModManager.UI.csproj -p:UseAppHost=true
```
3. Navigate to `macos`
```zsh
cd macos
```
4. Run the following command to create the app bundle.
```zsh
/bin/bash generate-bundle.bash
```
5. Navigate to `../output/macos`
6. Run `HedgeModManager.app`
