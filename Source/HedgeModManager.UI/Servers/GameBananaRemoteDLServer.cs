using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.Foundation;
using HedgeModManager.GameBanana;
using HedgeModManager.UI.CLI;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Servers;

public partial class GameBananaRemoteDLServer(MainWindowViewModel viewModel) : ObservableObject
{
    private readonly MainWindowViewModel _viewModel = viewModel;

    [ObservableProperty] private int _statusCode = 0;
    [ObservableProperty] private string _statusMessage = "Status.Text.NotRunning";
    [ObservableProperty] private string _statusArgument = "";

    public CancellationTokenSource ServerCancellationTokenSource { get; set; } = new();
    public DateTime LastPoll { get; set; } = DateTime.MinValue;

    public async Task StartServerAsync()
    {
        if (StatusCode == 1)
            return;
        SetServerStatus(1);
        try
        {
            LastPoll = DateTime.MinValue;
            var refreshTime = TimeSpan.FromSeconds(60);
            var c = ServerCancellationTokenSource.Token;
            Logger.Debug("Starting GameBanana Remote Install server");
            while (!c.IsCancellationRequested)
            {
                if (DateTime.Now - LastPoll < refreshTime)
                {
                    var remaining = refreshTime - (DateTime.Now - LastPoll);

                    SetServerStatus(1, null, $"{remaining.Seconds}s");
                    await Task.Delay(250, c);
                    continue;
                }

                string memberID = _viewModel.Config.Integrations.GameBananaRemoteDLMemberID;
                string secretKey = _viewModel.Config.Integrations.GameBananaRemoteDLSecretKey;
                var uris = await GameBananaAPI.FetchRemoteInstallQueue(memberID, secretKey, Program.ApplicationName);
                LastPoll = DateTime.Now;
                if (uris == null)
                {
                    Logger.Error("Failed to fetch GameBanana Remote Install queue");
                    SetServerStatus(3);
                    continue;
                }

                _ = Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    foreach (var uri in uris)
                    {
                        try
                        {
                            Logger.Debug($"Processing \"{uri}\"");
                            var args = CommandLine.ParseArguments(["--schema", uri]);
                            var (continueStartup, commands) = CommandLine.ExecuteArguments(args);
                            await _viewModel.ProcessCommandsAsync(commands);
                        }
                        catch (Exception e)
                        {
                            _viewModel.OpenErrorMessage("Modal.Title.UnknownError", "Modal.Message.UnknownError",
                                "Failed to process message", e);
                        }
                    }
                });
            }
            Logger.Debug("GameBanana RemoteDL server closed");
            SetServerStatus(0);
        }
        catch (OperationCanceledException)
        {
            SetServerStatus(0);
        }
        catch (Exception e)
        {
            Logger.Error("GameBanana RemoteDL server stopped due to error");
            Logger.Error(e);
            SetServerStatus(3);
        }
        ServerCancellationTokenSource = new();
    }

    public void StopServer()
    {
        if (StatusCode == 1)
        {
            SetServerStatus(2);
            ServerCancellationTokenSource.Cancel();
        }
    }

    public void SetServerStatus(int status, string? message = null, string? statusArgument = null)
    {
        StatusCode = status;
        if (message != null)
        {
            StatusMessage = message;
        }
        else
        {
            StatusMessage = status switch
            {
                0 => "Status.Text.NotRunning",
                1 => statusArgument != null ? "Status.Text.Running1" : "Status.Text.Running",
                2 => "Status.Text.Stopping",
                3 => "Status.Text.Error",
                _ => "Status.Text.Unknown"
            };
        }
        if (statusArgument != null)
            StatusArgument = statusArgument;
    }
}
