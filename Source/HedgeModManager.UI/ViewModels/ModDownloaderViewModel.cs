using Avalonia.Controls.Documents;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.Models;
using System;
using System.Threading.Tasks;
using static HedgeModManager.UI.Languages.Language;

namespace HedgeModManager.UI.ViewModels;

public partial class ModDownloaderViewModel : ViewModelBase
{
    [ObservableProperty] private bool _ready = false;
    [ObservableProperty] private bool _loading = true;
    [ObservableProperty] private string _title = "Common.Text.Loading";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private ModDownloadInfo? _downloadInfo;

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
    }
}
