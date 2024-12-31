using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ModDownloaderModal : WindowModal
{
    public ModDownloaderViewModel ViewModel { get; set; }

    // Preview only
    public ModDownloaderModal()
    {
        ViewModel = new(async () =>
        {
            await Task.Delay(5);
            return new()
            {
                GameID = "SonicOrigins",
                Name = "Test Mod",
                Description = "Markdown:\n - Item 1\n - Item 2\n\nNot an item.\n\nHTML:<br/><b>Bold text</b><br/><span>Not so bold text</span>",
                Images = ["https://images.gamebanana.com/img/Webpage/Game/Profile/Background/629d9d63eb125.png"],
            };
        });
        InitializeComponent();
    }

    public ModDownloaderModal(Func<Task<ModDownloadInfo?>> downloadInfoCallback)
    {
        ViewModel = new(downloadInfoCallback);
        InitializeComponent();
    }

    private async void OnInitialized(object? sender, RoutedEventArgs e)
    {
        Bind(TitleProperty, new Binding
        {
            Source = ViewModel,
            Path = "Title"
        });

        await ViewModel.StartInfoDownload();
    }

    private async void OnDownloadClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            Close();

            var downloadInfo = ViewModel.DownloadInfo;
            if (downloadInfo == null)
                return;

            // Get desired game
            var uiGame = viewModel.SelectedGame;
            if (uiGame == null || uiGame.Game.Name != downloadInfo.GameID)
            {
                var desiredGame = viewModel.Games
                .FirstOrDefault(x => x.Game.Name == downloadInfo.GameID);
                if (desiredGame is not UIGame desiredUIGame)
                {
                    Logger.Error($"Game {downloadInfo.GameID} not found");
                    var messageBox = new MessageBoxModal("Modal.Title.InstallError", 
                        Localize("Modal.Message.GameMissingError",
                        downloadInfo.Name, Localize($"Common.Game.{downloadInfo.GameID}")));
                    messageBox.AddButton("Common.Button.OK", (s, e) => messageBox.Close());
                    messageBox.Open(viewModel);
                    return;
                }
                uiGame = desiredUIGame;
                await uiGame.Game.InitializeAsync();
            }
            if (uiGame.Game.ModDatabase is not ModDatabaseGeneric modsDB)
            {
                Logger.Error("Only ModDatabaseGeneric is supported for installing mods");
                return;
            }

            string downloadPath = Path.GetTempFileName();

            await new Download(Localize("Download.Text.DownloadMod", downloadInfo.Name), true, -1).OnRun(async (d, c) =>
            {
                var progress = d.CreateProgress();

                Logger.Information($"Downloading {downloadInfo.Name}...");
                bool completed = await Network.DownloadFile(downloadInfo.DownloadURL, downloadPath, null, progress, c);

                if (!completed)
                {
                    viewModel.OpenErrorMessage("Modal.Title.DownloadError", "Modal.Message.DownloadError", $"Failed to download {downloadInfo.Name}");
                    d.Destroy();
                    return;
                }

                d.Name = Localize("Download.Text.InstallMod", downloadInfo.Name);
                progress.ReportMax(-1);

                Logger.Debug($"Started installing {downloadInfo.Name}");
                await modsDB.InstallModFromArchive(downloadPath, progress);
                Logger.Information($"Finished installing {downloadInfo.Name}");
                if (uiGame == viewModel.SelectedGame)
                    Dispatcher.UIThread.Invoke(viewModel.RefreshUI);
            }).OnCancel((d) =>
            {
                Logger.Debug("Mod install cancelled");
                return Task.CompletedTask;
            }).OnError((d, e) =>
            {
                viewModel.OpenErrorMessage("Modal.Title.DownloadError", "Modal.Message.DownloadError", $"Failed to download/install {downloadInfo.Name}");
                return Task.CompletedTask;
            }).OnFinally((d) =>
            {
                try { if (File.Exists(downloadPath)) File.Delete(downloadPath); } catch { }
                d.Destroy();
                return Task.CompletedTask;
            })
            .RunAsync(viewModel);
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}