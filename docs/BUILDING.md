# Building from source

## 1. Clone the repository
```zsh
git clone https://github.com/hedge-dev/HedgeModManager.git
```

## 2. Build the project

###  Macos

1. Navigate to the root of the project
2. Run the build command
```zsh
dotnet publish -p:PublishProfile=osx-arm64 -c Release -p:AssemblyVersion=8.0.0 -p:FileVersion=8.0.0 -o ./output/osx-arm64 ./Source/HedgeModManager.UI/HedgeModManager.UI.csproj -p:UseAppHost=true && cd macos && /bin/bash generate-bundle.bash com.hedge_dev.hedgemodmanager 8.0.3
```
3. Navigate to /output/osx-arm64
4. Start HedgeModManager.app
