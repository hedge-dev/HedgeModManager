using CommunityToolkit.Mvvm.ComponentModel;
using HedgeModManager.UI.ViewModels;

namespace HedgeModManager.UI.Models;

public partial class Download : ObservableObject
{
    private readonly List<DownloadProgress> _progresses = [];

    [ObservableProperty] private string _name = "Download Name";
    [ObservableProperty] private double _progress = 0d;
    [ObservableProperty] private double _progressMax = 100d;
    [ObservableProperty] private bool _running = false;
    [ObservableProperty] private bool _destroyed = false;
    [ObservableProperty] private bool _customTitle = false;

    public Func<Download, CancellationToken, Task>? RunCallback;
    public Func<Download, Task>? FinallyCallback;
    public Func<Download, Exception, Task>? ErrorCallback;
    public Func<Download, Task>? CanceledCallback;
    public CancellationTokenSource CancelToken = new();
    public MainWindowViewModel? MainViewModel;

    public Download(string name, bool customTitle = false, long progressMax = 100L)
    {
        Name = name;
        CustomTitle = customTitle;
        ProgressMax = progressMax;
    }

    public void Cancel()
    {
        CancelToken.Cancel();
        Running = false;
    }

    public Download OnRun(Func<Download, CancellationToken, Task> callback)
    {
        RunCallback = callback;
        return this;
    }

    public Download OnFinally(Func<Download, Task> callback)
    {
        FinallyCallback = callback;
        return this;
    }

    public Download OnError(Func<Download, Exception, Task> callback)
    {
        ErrorCallback = callback;
        return this;
    }

    public Download OnCancel(Func<Download, Task> callback)
    {
        CanceledCallback = callback;
        return this;
    }

    public Download Run(MainWindowViewModel? mainViewModel)
    {
        Task.Run(async () => await RunAsync(mainViewModel));
        return this;
    }

    public async Task RunAsync(MainWindowViewModel? mainViewModel)
    {
        if (Running)
            return;

        Running = true;
        MainViewModel = mainViewModel;
        MainViewModel?.AddDownload(this);

        try
        {
            if (RunCallback != null)
                await RunCallback(this, CancelToken.Token);
        }
        catch (TaskCanceledException)
        {
            if (CanceledCallback != null)
                await CanceledCallback(this);
        }
        catch (Exception e)
        {
            if (ErrorCallback != null)
            {
                await ErrorCallback(this, e);
            }
            else
            {
                Logger.Error(e);
                Logger.Error($"Download {Name} failed with exception");
            }
        }
        finally
        {
            if (FinallyCallback != null)
                await FinallyCallback(this);
            Running = false;
        }
    }

    public void Destroy()
    {
        if (Running)
            Cancel();
        CancelToken.Dispose();
        if (MainViewModel != null)
            MainViewModel.Message = "";
        Destroyed = true;
    }

    public void UpdateProgress()
    {
        if (_progresses.Count == 0)
            return;

        double progress = 0;
        double progressMax = 0;
        long largestProgressMax = _progresses.Max(x => x.ProgressMax);

        progressMax = largestProgressMax * _progresses.Count;
        foreach (var progresses in _progresses)
        {
            if (progresses.ProgressMax == 0)
                continue;
            double ratio = (double)progresses.Progress / progresses.ProgressMax;
            progress += ratio * largestProgressMax;
        }

        Progress = progress;
        ProgressMax = progressMax;
    }

    public DownloadProgress CreateProgress()
    {
        var progress = new DownloadProgress();
        progress.PropertyChanged += (s, e) => UpdateProgress();
        _progresses.Add(progress);
        return progress;
    }

    public void Report(long value) => Progress = value;

    public void ReportMax(long value) => ProgressMax = value;

    public partial class DownloadProgress : ObservableObject, IProgress<long>
    {
        [ObservableProperty] public long _progress = 0;
        [ObservableProperty] public long _progressMax = -1;

        public void Report(long value) => Progress = value;
        public void ReportAdd(long value) => Progress += value;

        public void ReportMax(long value) => ProgressMax = value;
    }
}
