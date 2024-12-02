using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using HedgeModManager.UI.Models;
using HedgeModManager.UI.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.Controls.Modals;

public partial class ModDownloaderModal : UserControl
{
    public ModDownloaderViewModel ViewModel { get; set; }

    // Preview only
    public ModDownloaderModal()
    {
        ViewModel = new(async () =>
        {
            await Task.Delay(5000);
            return new()
            {
                Name = "Test Mod"
            };
        });

        InitializeComponent();
    }

    public ModDownloaderModal(Func<Task<ModDownloadInfo?>> downloadInfoCallback)
    {
        ViewModel = new(downloadInfoCallback);

        InitializeComponent();
    }

    public void Open(MainWindowViewModel viewModel)
    {
        viewModel.Modals.Add(new Modal(this, new Thickness(0)));
    }

    public void Close()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var modalInstance = viewModel.Modals.FirstOrDefault(x => x.Control == this);
            if (modalInstance != null)
                viewModel.Modals.Remove(modalInstance);
        }
    }

    private async void OnInitialized(object? sender, EventArgs e)
    {
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

            new Download(downloadInfo.Name, 1).OnRun(async (d, c) =>
            {
                Logger.Information($"Downloading {downloadInfo.Name}...");
                var client = new HttpClient();
                var response = await client.GetAsync(downloadInfo.DownloadURL,
                    HttpCompletionOption.ResponseHeadersRead, c);
                response.EnsureSuccessStatusCode();

                var downloadProgress = d.CreateProgress();
                var installProgress = d.CreateProgress();

                downloadProgress.ReportMax(response.Content.Headers.ContentLength ?? 0L);
                installProgress.ReportMax(0);

                using var stream = await response.Content.ReadAsStreamAsync(c);
                using var fileStream = File.Create(downloadPath);

                var buffer = new byte[1048576];
                int bytesRead = 0;
                long totalBytesRead = 0L;

                while ((bytesRead = await stream.ReadAsync(buffer, c)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), c);
                    downloadProgress.Report(totalBytesRead += bytesRead);
                }
                fileStream.Close();
                Logger.Debug($"Started installing {downloadInfo.Name}");

                // Install mod
                await modsDB.InstallModFromArchive(downloadPath, installProgress);

            }).OnComplete((d) =>
            {
                Logger.Information($"Finished installing {downloadInfo.Name}");
                if (uiGame == viewModel.SelectedGame)
                    Dispatcher.UIThread.Invoke(viewModel.RefreshUI);
                try { if (File.Exists(downloadPath)) File.Delete(downloadPath); } catch { }
                d.Destroy();
                return Task.CompletedTask;
            }).OnCancel((d) =>
            {
                Logger.Debug("Mod install cancelled");
                return Task.CompletedTask;
            }).OnError(async (d, e) =>
            {
                Logger.Error($"Failed to install {downloadInfo.Name}");

                try { if (File.Exists(downloadPath)) File.Delete(downloadPath); } catch { }
                await Task.Delay(5000);
                d.Destroy();
            })
            .Run(viewModel);
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}