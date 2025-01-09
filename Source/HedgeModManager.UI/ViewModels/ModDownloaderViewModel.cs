using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Models;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.ViewModels;

public partial class ModDownloaderViewModel : ViewModelBase
{
    [ObservableProperty] private bool _ready = false;
    [ObservableProperty] private bool _loading = true;
    [ObservableProperty] private string _title = "Modal.Title.DownloadModLoading";
    [ObservableProperty] private string _name = "Common.Text.Loading";
    [ObservableProperty] private string _author = "Common.Text.Loading";
    [ObservableProperty] private string _targetGameName = "Common.Text.Loading";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private ModDownloadInfo? _downloadInfo;
    [ObservableProperty] private IImage? _banner;
    [ObservableProperty] private IImage? _gameIcon;

    [ObservableProperty] private InlineCollection _creditsInline = [];

    private Func<Task<ModDownloadInfo?>>? _downloadInfoCallback;

    public ModDownloaderViewModel()
    {
        Description = Localize("Common.Text.Loading");
    }

    public ModDownloaderViewModel(Func<Task<ModDownloadInfo?>> downloadInfoCallback) : this()
    {
        DownloadInfo = new();
        _downloadInfoCallback = downloadInfoCallback;
    }

    public async Task StartInfoDownload()
    {
        if (_downloadInfoCallback != null)
            DownloadInfo = await _downloadInfoCallback();
        if (DownloadInfo != null)
        {
            _ = Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (DownloadInfo.Images.Count > 0)
                    Banner = await Utils.DownloadBitmap(DownloadInfo.Images[0]);
            });
            Ready = true;
            Loading = false;
            Update();
        }
    }

    public void Update()
    {
        if (DownloadInfo == null)
            return;

        var creditsInline = new InlineCollection();
        foreach (var authorGroup in DownloadInfo.Authors)
        {
            creditsInline.Add(new Run($"{authorGroup.Key}:\n") { FontWeight = FontWeight.Bold });
            foreach (var author in authorGroup.Value)
            {
                creditsInline.Add(new Run($"   {author.Name}\n"));
                creditsInline.Add(new Run($"     {author.Description}\n") { FontSize = 12 });
            }
            creditsInline.Add(new LineBreak());
        }
        CreditsInline = creditsInline;
        Description = Utils.ConvertToHTML(DownloadInfo.Description);
        Title = Localize("Modal.Title.DownloadMod", DownloadInfo.Name);
        Name = DownloadInfo.Name;
        if (DownloadInfo.Authors.Count > 0)
            Author = DownloadInfo.Authors.First().Key;
        else
            Author = "Unknown Author";

        GameIcon = Games.GetIcon(DownloadInfo.GameID);
        TargetGameName = Localize("Modal.Text.TargetGame", Localize($"Common.Game.{DownloadInfo.GameID}"));
    }
}
