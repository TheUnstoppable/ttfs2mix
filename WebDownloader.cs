using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ttfs2Mix
{
    public static class WebDownloader
    {
        public static async Task<byte[]> GetBytesAsync(string Url)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", $"ttfs2mix-{Program.Version}");
            client.DefaultRequestHeaders.Add("Referrer", ProgressStatisticClass.CurrentPackage);

            HttpResponseMessage response = await client.GetAsync(Url);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Server returned HTTP {(int)response.StatusCode}.", null, response.StatusCode);
            }
            else
            {
                return await response.Content.ReadAsByteArrayAsync();
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
}
