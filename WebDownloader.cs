namespace Ttfs2Mix;

public class WebDownloaderProgress
{
    public long DownloadedBytes { get; init; }
    public long TotalSize { get; init; }
    public long Speed { get; init; }
}

public static class WebDownloader
{
    public static async Task<byte[]> GetBytesAsync(string Url, string Referrer, IProgress<WebDownloaderProgress>? Progress = null)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", $"ttfs2mix-{Ttfs2Mix.Version}");
        client.DefaultRequestHeaders.Add("Referrer", Referrer);

        HttpResponseMessage response = await client.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException($"Server returned HTTP {(int)response.StatusCode}.", null, response.StatusCode);
        }
        else
        {
            int bytesRead = 0;
            byte[] buffer = new byte[81920];
            using (var ms = new MemoryStream())
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                Stopwatch sw = Stopwatch.StartNew();
                long lastBytes = 0;
                long lastTimestamp = sw.ElapsedTicks;
                long lastSpeed = 0;
                
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);

                    long now = sw.ElapsedTicks;
                    double elapsed = (double)(now - lastTimestamp) / Stopwatch.Frequency;

                    if (elapsed >= 0.5)
                    {
                        long downloaded = ms.Length - lastBytes;
                        lastSpeed = (long)(downloaded / elapsed);
                        lastBytes = ms.Length;
                        lastTimestamp = now;
                    }
                    
                    Progress?.Report(new WebDownloaderProgress
                    {
                        DownloadedBytes = ms.Length,
                        TotalSize = response.Content.Headers.ContentLength ?? -1,
                        Speed = lastSpeed
                    });
                }

                return ms.ToArray();
            }
        }
    }

    //https://stackoverflow.com/a/14488941/5791443
    public static string ParseSize(long value, int decimalPlaces = 2)
    {
        if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException(nameof(decimalPlaces)); }
        if (value < 0) { return "-" + ParseSize(-value, decimalPlaces); }
        if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

        // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
        int mag = (int)Math.Log(value, 1024);

        // 1L << (mag * 10) == 2 ^ (10 * mag) 
        // [i.e. the number of bytes in the unit corresponding to mag]
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        // make adjustment when the value is large enough that
        // it would round up to 1000 or more
        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            SizeSuffixes[mag]);
    }

    //https://stackoverflow.com/a/14488941/5791443
    internal static readonly string[] SizeSuffixes =
        { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
}