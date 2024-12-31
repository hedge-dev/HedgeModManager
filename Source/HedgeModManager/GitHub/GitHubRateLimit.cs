using System.Net.Http.Headers;

namespace HedgeModManager.GitHub;

public class GitHubRateLimit
{
    public int Limit { get; set; } = 0;
    public int Remaining { get; set; } = 0;
    public int Used { get; set; } = 0;
    public DateTime Reset { get; set; } = DateTime.Now;

    public static GitHubRateLimit FromHeaders(HttpHeaders headers)
    {
        var rateLimit = new GitHubRateLimit();
        if (headers.TryGetValues("X-RateLimit-Limit", out var limit))
            rateLimit.Limit = int.Parse(limit.FirstOrDefault() ?? "0");
        if (headers.TryGetValues("X-RateLimit-Remaining", out var remaining))
            rateLimit.Remaining = int.Parse(remaining.FirstOrDefault() ?? "0");
        if (headers.TryGetValues("X-RateLimit-Used", out var used))
            rateLimit.Used = int.Parse(used.FirstOrDefault() ?? "0");
        if (headers.TryGetValues("X-RateLimit-Reset", out var reset))
            rateLimit.Reset = DateTimeOffset.FromUnixTimeSeconds(long.Parse(reset.FirstOrDefault() ?? "0")).DateTime;
        return rateLimit;
    }
}
