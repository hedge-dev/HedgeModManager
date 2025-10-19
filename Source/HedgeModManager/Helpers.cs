using System.Security.Cryptography;

namespace HedgeModManager;

public static class Helpers
{
    public static bool IsFlatpak => Environment.GetEnvironmentVariable("FLATPAK_ID") != null;

    public static async Task<string> GetFileHashAsync(string filePath, HashAlgorithm hashAlgorithm, CancellationToken? c = default)
    {
        CancellationToken cancellationToken = c ?? CancellationToken.None;
        using FileStream stream = File.OpenRead(filePath);
        byte[] hash = await hashAlgorithm.ComputeHashAsync(stream, cancellationToken);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public static async Task<bool> CheckFileHashAsync(string filePath, string hash, HashAlgorithm hashAlgorithm, CancellationToken? c = default)
    {
        return (await GetFileHashAsync(filePath, hashAlgorithm, c)).Equals(hash, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Converts all backslashes to forward slashes
    /// </summary>
    public static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    /// <summary>
    /// Ensures that the specified path ends with a trailing forward slash "/"
    /// </summary>
    public static string EnsureTrailingSlash(string baseHost)
    {
        return baseHost.TrimEnd('/') + '/';
    }

    /// <summary>
    /// Combines a base URL path and a file path into a single URL
    /// </summary>
    public static string CombineURL(string baseHost, string path)
    {
        return baseHost.TrimEnd('/') + '/' + path.Replace('\\', '/').TrimStart('/');
    }

    /// <summary>
    /// Combines a base URI and a file path into a single URL
    /// </summary>
    public static string CombineURL(Uri basePath, string path)
    {
        return basePath.OriginalString.TrimEnd('/') + '/' + path.Replace('\\', '/').TrimStart('/');
    }

    /// <summary>
    /// Encodes a URL except for forward slashes
    /// </summary>
    public static string EncodeURL(string url)
    {
        return Uri.EscapeDataString(url).Replace("%2F", "/");
    }
}
