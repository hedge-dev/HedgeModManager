using System.Text.Json;

namespace HedgeModManager;

public static class Network
{
    public static string UserAgent = $"Mozilla/5.0 (compatible; HedgeModManager)";
    public static HttpClient Client = new();

    public static void Initialize()
    {
        Client?.Dispose();
        Client = new()
        {
            DefaultRequestHeaders = {{ "User-Agent", UserAgent }}
        };
    }

    public static Uri? CreateUri(string? uri) =>
            string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);

    /// <summary>
    /// Sends a GET request to the specified URI and deserializes the response into the specified type.
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="url">URL to send request to</param>
    /// <param name="c">Cancellation token</param>
    /// <returns>Converted type from response, default if failed</returns>
    public static async Task<T?> Get<T>(string url, CancellationToken c = default)
        => await Get<T>(CreateUri(url), c);


    /// <summary>
    /// Sends a GET request to the specified URI and deserializes the response into the specified type.
    /// </summary>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <param name="uri">URI to send request to</param>
    /// <param name="c">Cancellation token</param>
    /// <returns>Converted type from response, default if failed</returns>
    public static async Task<T?> Get<T>(Uri? uri, CancellationToken c = default)
    {
        Logger.Debug($"Sending GET request to {uri} for {typeof(T).Name}");
        var response = await Client.GetAsync(uri, c);
        if (!response.IsSuccessStatusCode)
        {
            Logger.Debug($"Got error status code: {response.StatusCode}");
            return default;
        }
        var json = await response.Content.ReadAsStringAsync(c);
        Logger.Debug("Received data, deserialising...");
        return JsonSerializer.Deserialize<T>(json);
    }

    /// <summary>
    /// Downloads a file to disk.
    /// 
    /// If cache is enabled, the file will be read from the cache first.
    /// </summary>
    /// <param name="url">URL to send request to</param>
    /// <param name="cacheFileName">Name of file to be stored in the cache folder. Set empty ot null for no caching</param>
    /// <param name="progress">IProgress<long> to report download progress to</param>
    /// <param name="c">Cancellation token</param>
    /// <returns>A read-only FileStream from cache or MemoryStream from download</returns>
    public static async Task<bool> DownloadFile(string url, string outPath, string? cacheFileName, IProgress<long>? progress, CancellationToken c = default) =>
        await DownloadFile(CreateUri(url), outPath, cacheFileName, progress, c);


    /// <summary>
    /// Downloads a file to disk.
    /// 
    /// If cache is enabled, the file will be read from the cache first.
    /// </summary>
    /// <param name="uri">URI to send request to</param>
    /// <param name="cacheFileName">Name of file to be stored in the cache folder. Set empty ot null for no caching</param>
    /// <param name="progress">IProgress<long> to report download progress to</param>
    /// <param name="c">Cancellation token</param>
    /// <returns>A read-only FileStream from cache or MemoryStream from download</returns>
    public static async Task<bool> DownloadFile(Uri? uri, string outPath, string? cacheFileName, IProgress<long>? progress, CancellationToken c = default)
    {
        if (!string.IsNullOrEmpty(cacheFileName))
        {
            Logger.Debug($"Checking cache for {uri} ({cacheFileName})");
            string cachePath = Path.Combine(Paths.GetCachePath(), cacheFileName!);
            if (File.Exists(cachePath))
            {
                // TODO: Find better solution as this can fail if paths are written differenly but are the same file
                if (!cachePath.ToLowerInvariant().Equals(outPath.ToLowerInvariant()))
                    File.Copy(cachePath, outPath, true);
                return true;
            }
        }

        var stream = await Download(uri, cacheFileName, progress, c);
        if (stream == null)
        {
            return false;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        using var fileStream = File.Create(outPath);
        await stream.CopyToAsync(fileStream, c);
        stream.Dispose();
        return true;
    }

    /// <summary>
    /// Downloads a file into memory and optionally caches it to disk.
    /// 
    /// If cache is enabled, the file will be read from the cache first.
    /// </summary>
    /// <param name="url">URL to send request to</param>
    /// <param name="cacheFileName">Name of file to be stored in the cache folder. Set empty ot null for no caching</param>
    /// <param name="progress">IProgress<long> to report download progress to</param>
    /// <param name="c">Cancellation token</param>
    /// <returns>A read-only FileStream from cache or MemoryStream from download</returns>
    public static async Task<Stream?> Download(string url, string? cacheFileName, IProgress<long>? progress, CancellationToken c = default) =>
        await Download(CreateUri(url), cacheFileName, progress, c);

    /// <summary>
    /// Downloads a file into memory and optionally caches it to disk.
    /// 
    /// If cache is enabled, the file will be read from the cache first.
    /// </summary>
    /// <param name="uri">URI to send request to</param>
    /// <param name="cacheFileName">Name of file to be stored in the cache folder. Set empty ot null for no caching</param>
    /// <param name="progress">IProgress<long> to report download progress to</param>
    /// <param name="c">Cancellation token</param>
    /// <returns>A read-only FileStream from cache or MemoryStream from download</returns>
    public static async Task<Stream?> Download(Uri? uri, string? cacheFileName, IProgress<long>? progress, CancellationToken c = default)
    {
        if (uri == null)
            return null;
        bool useCache = !string.IsNullOrEmpty(cacheFileName);
        string? cachePath = null;
        if (useCache)
        {
            Logger.Debug($"Checking cache for {uri} ({cacheFileName})");
            cachePath = Path.Combine(Paths.GetCachePath(), cacheFileName!);
            if (File.Exists(cachePath))
                return File.OpenRead(cachePath);
        }

        Logger.Debug($"Beginning Download {uri}");
        Logger.Debug("Waiting for headers...");
        var response = await Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, c);
        if (!response.IsSuccessStatusCode)
        {
            Logger.Debug($"Got error status code: {response.StatusCode}");
            return null;
        }

        var memoryStream = new MemoryStream();
        using var stream = await response.Content.ReadAsStreamAsync(c);
        Logger.Debug("Opened download stream");

        if (progress != null)
        {
            if (response.Content.Headers.ContentLength.HasValue)
                progress.ReportMax(response.Content.Headers.ContentLength.Value);
            else
                progress.ReportMax(-1);
        }

        var buffer = new byte[4096];
        int bytesRead = 0;
        long totalBytesRead = 0L;

        while ((bytesRead = await stream.ReadAsync(buffer, c)) > 0)
        {
            await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), c);
            progress?.Report(totalBytesRead += bytesRead);
        }

        if (useCache)
        {
            Logger.Debug("Writing download to cache...");
            memoryStream.Position = 0;
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            using var fileStream = File.Create(cachePath!);
            await memoryStream.CopyToAsync(fileStream, c);
        }

        memoryStream.Position = 0;
        Logger.Debug("Download completed");
        return memoryStream;
    }

    /// <summary>
    /// Downloads a file into memory as a string
    /// </summary>
    /// <param name="url">URL to send request to</param>
    /// <param name="progress">IProgress<long> to report download progress to</param>
    /// <param name="c">Cancellation token</param>
    /// <returns>A string containing the data from the download</returns>
    public static async Task<string?> DownloadString(string url, CancellationToken c = default) =>
        await DownloadString(CreateUri(url), c);

    /// <summary>
    /// Downloads a file into memory as a string
    /// </summary>
    /// <param name="uri">URI to send request to</param>
    /// <param name="progress">IProgress<long> to report download progress to</param>
    /// <param name="c">Cancellation token</param>
    /// <returns>A string containing the data from the download</returns>
    public static async Task<string?> DownloadString(Uri? uri, CancellationToken c = default)
    {
        if (uri == null)
            return null;

        Logger.Debug($"Beginning Download {uri}");
        Logger.Debug("Waiting for headers...");
        var response = await Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, c);
        if (!response.IsSuccessStatusCode)
        {
            Logger.Debug($"Got error status code: {response.StatusCode}");
            return null;
        }

        Logger.Debug("Downloading string...");
        string result = await response.Content.ReadAsStringAsync(c);
        Logger.Debug("Download completed");
        return result;
    }
}
