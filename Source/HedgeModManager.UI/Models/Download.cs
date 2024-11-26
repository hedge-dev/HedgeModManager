using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace HedgeModManager.UI.Models;

public partial class Download : ObservableObject, IProgress<long>
{
    [ObservableProperty] private string _name = "Download Name";
    [ObservableProperty] private long _progress = 0;
    [ObservableProperty] private long _progressMax = 100;
    [ObservableProperty] private bool _running = false;

    public Collection<Download>? DownloadCollection;
    public Func<Download, CancellationToken, Task>? RunCallback;
    public Func<Download, Task>? CompleteCallback;
    public Func<Download, Exception, Task>? ErrorCallback;
    public Func<Download, Task>? CanceledCallback;
    public CancellationTokenSource CancelToken = new();

    public Download(string name, long progressMax)
    {
        Name = name;
        ProgressMax = progressMax;
    }

    public void Cancel()
    {
        CancelToken.Cancel();
    }

    public Download OnRun(Func<Download, CancellationToken, Task> callback)
    {
        RunCallback = callback;
        return this;
    }

    public Download OnComplete(Func<Download, Task> callback)
    {
        CompleteCallback = callback;
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

    public Download Run(Collection<Download>? downloadCollection = null)
    {
        if (Running)
            return this;

        Running = true;

        DownloadCollection = downloadCollection;
        DownloadCollection?.Add(this);

        Task.Run(async () =>
        {
            try
            {
                if (RunCallback != null)
                    await RunCallback(this, CancelToken.Token);
                if (CompleteCallback != null)
                    await CompleteCallback(this);
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
                Running = false;
            }
        });
        return this;
    }

    public void Destroy()
    {
        if (Running)
            Cancel();
        CancelToken.Dispose();
        DownloadCollection?.Remove(this);
    }

    public void Report(long value) => Progress = value;

    public void ReportMax(long value) => ProgressMax = value;
}
