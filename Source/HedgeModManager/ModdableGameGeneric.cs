﻿namespace HedgeModManager;
using CoreLib;
using Foundation;
using HedgeModManager.CodeCompiler;
using HedgeModManager.Diagnostics;
using HedgeModManager.IO;
using HedgeModManager.Properties;
using System;

public class ModdableGameGeneric : IModdableGameTDatabase<ModDatabaseGeneric>, IModdableGameTConfiguration<ModLoaderConfiguration>
{
    private string _nativeOS = "Windows";

    public IGame BaseGame { get; }

    public string Platform => BaseGame.Platform;
    public string ID => BaseGame.ID;
    public string Name { get; set; }
    public string Root { get; set; }
    public string? Executable { get; set; }
    public string DefaultDatabaseDirectory { get; set; } = "mods";
    public string ModLoaderName { get; init; } = "None";
    public string? PrefixRoot => BaseGame.PrefixRoot;
    public bool SupportsDirectLaunch { get; set; }
    public bool SupportsLauncher { get; set; }
    public bool Is64Bit { get; set; } = true;
    public string? LaunchCommand { get; set; } = null;
    public ModDatabaseGeneric ModDatabase { get; } = new ModDatabaseGeneric();
    public ModLoaderConfiguration ModLoaderConfiguration { get; set; } = new ModLoaderConfiguration();
    public ModLoaderGeneric? ModLoader { get; set; }
    public string NativeOS
    {
        get => _nativeOS;
        set
        {
            _nativeOS = value;
            ModDatabase.NativeOS = value;
            ModLoaderConfiguration.NativeOS = value;
        }
    }

    public ModdableGameGeneric(IGame game)
    {
        BaseGame = game;
        Name = game.Name;
        Root = game.Root;
        Executable = game.Executable;
        SupportsDirectLaunch = game.SupportsDirectLaunch;
        SupportsLauncher = game.SupportsLauncher;
        NativeOS = game.NativeOS;
    }

    public async Task<Diff> UpdateCodes(bool dryRun = false, string? downloadUrl = null)
    {
        downloadUrl ??= Resources.CommunityCodesURL;
        if (downloadUrl.EndsWith('/'))
            downloadUrl += $"{Name}.hmm?t={DateTime.Now:yyyyMMddHHmmss}";

        string codesPath = Path.Combine(PathEx.GetDirectoryName(ModLoaderConfiguration.DatabasePath).ToString(), ModDatabaseGeneric.MainCodesFileName);

        var remoteContents = await Network.Client.GetStringAsync(downloadUrl);

        Diff? diff = null;
        if (File.Exists(codesPath))
        {
            var localCodes = CodeFile.FromText(File.ReadAllText(codesPath));
            var remoteCodes = CodeFile.FromText(remoteContents);

            diff = remoteCodes.CalculateDiff(localCodes);
        }

        if (!dryRun)
        {
            Directory.CreateDirectory(PathEx.GetDirectoryName(codesPath).ToString());
            File.WriteAllText(codesPath, remoteContents);
        }

        return diff ?? new Diff();
    }

    public async Task InitializeAsync()
    {
        try
        {
            // TODO: Change this
            await ModLoaderConfiguration.Load(Path.Combine(Root, "cpkredir.ini"));
            string directory = PathEx.GetDirectoryName(ModLoaderConfiguration.DatabasePath).ToString();
            if (!Directory.Exists(directory))
            {
                ModLoaderConfiguration.DatabasePath = string.Empty;
            }
        }
        catch
        {
            ModLoaderConfiguration.DatabasePath = string.Empty;
        }

        if (string.IsNullOrEmpty(ModLoaderConfiguration.DatabasePath))
        {
            // mods / Mods workaround
            string mainPath = Path.Combine(Root, DefaultDatabaseDirectory, ModDatabaseGeneric.DefaultDatabaseName);
            string altPath = Path.Combine(Root, "Mods", ModDatabaseGeneric.DefaultDatabaseName);

            if (!File.Exists(mainPath) && File.Exists(altPath))
            {
                mainPath = altPath;
            }
            ModLoaderConfiguration.DatabasePath = mainPath;
        }

        ModDatabase.LoadDatabase(ModLoaderConfiguration.DatabasePath);
    }

    public async Task<bool> InstallModLoaderAsync()
    {
        if (ModLoader != null)
        {
            if (ModLoader.IsInstalled())
            {
                return await ModLoader.UninstallAsync();
            }
            else
            {
                return await ModLoader.InstallAsync();
            }
        }
        return true;
    }

    public bool IsModLoaderInstalled()
    {
        if (ModLoader != null)
        {
            return ModLoader.IsInstalled();
        }
        return true;
    }

    public async Task Run(string? launchArgs, bool useLauncher) =>
        await BaseGame.Run(launchArgs, useLauncher);
}