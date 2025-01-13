using AvaloniaEdit;
using System.Net;
using System.Text.Json;

namespace New_SSQE.ExternalUtils
{
    internal enum FileSource
    {
        Other,
        Roblox,
        Pulsus
    }

    internal class WebClient
    {
        private static readonly HttpClient client = new();
        private static readonly HttpClient impatientClient = new()
        {
            Timeout = TimeSpan.FromMilliseconds(30000)
        };
        private static readonly HttpClient robloxClient = new(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
        });
        private static readonly HttpClient redirectClient = new(new HttpClientHandler()
        {
            AllowAutoRedirect = false
        });

        public static string GetBeatmapURLFromRhythiaMapID(int id)
        {
            Task<string> result = Task.Run(async () =>
            {
                HttpRequestMessage request = new(HttpMethod.Post, "https://development.rhythia.com/api/getBeatmapPage")
                {
                    Content = new StringContent("{\"id\":" + id + ",\"session\":\"\"}")
                };

                using HttpResponseMessage response = await client.SendAsync(request);
                using HttpContent content = response.Content;

                return await content.ReadAsStringAsync();
            });

            Dictionary<string, JsonElement> json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.Result) ?? new();
            Dictionary<string, JsonElement> beatmapData = json.TryGetValue("beatmap", out JsonElement value) ? value.Deserialize<Dictionary<string, JsonElement>>() ?? new() : new();

            return beatmapData.TryGetValue("beatmapFile", out JsonElement file) ? file.GetString() ?? "" : "";
        }

        public static void GetDifficultyMetrics(string mapData, out float sr, out Dictionary<string, float> rp)
        {
            HttpStatusCode status = HttpStatusCode.Processing;

            Task<string> result = Task.Run(async () =>
            {
                HttpRequestMessage request = new(HttpMethod.Post, "https://development.rhythia.com/api/getRawStarRating")
                {
                    Content = new StringContent("{\"rawMap\":\"" + mapData + "\",\"session\":\"\"}")
                };

                using HttpResponseMessage response = await client.SendAsync(request);
                using HttpContent content = response.Content;

                status = response.StatusCode;
                return await content.ReadAsStringAsync();
            });

            string jsonResult = result.Result;

            if (status != HttpStatusCode.OK)
            {
                sr = 0;
                rp = new() { { "Upload Failed", (int)status } };
            }
            else
            {
                Dictionary<string, JsonElement> json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.Result) ?? new();
                Dictionary<string, JsonElement> beatmapData = json.TryGetValue("beatmap", out JsonElement value) ? value.Deserialize<Dictionary<string, JsonElement>>() ?? new() : new();

                sr = beatmapData.TryGetValue("starRating", out JsonElement rating) ? rating.GetSingle() : 0f;
                rp = beatmapData.TryGetValue("rp", out JsonElement points) ? points.Deserialize<Dictionary<string, float>>() ?? new() : new();
            }
        }

        public static string GetRedirect(string url)
        {
            string final = "";

            Task<string> result = Task.Run(async () =>
            {
                using HttpResponseMessage response = await redirectClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.Redirect)
                    final = (response.Headers.Location ?? new Uri("")).ToString();
            }).ContinueWith(t =>
            {
                return final;
            });

            return result.Result;
        }

        public static string DownloadString(string url)
        {
            Task<string> result = Task.Run(async () =>
            {
                using HttpResponseMessage response = await impatientClient.GetAsync(url);
                using HttpContent content = response.Content;

                return await content.ReadAsStringAsync();
            });

            if (result.Result != null)
            {
                return result.Result;
            }
            else
                throw new WebException();
        }

        public static void DownloadFile(string url, string location, FileSource source = FileSource.Other)
        {
            Task<HttpStatusCode> result = Task.Run(async () =>
            {
                HttpClient determined = source == FileSource.Roblox ? robloxClient : client;
                HttpRequestMessage request = new(HttpMethod.Get, url);
                if (source == FileSource.Roblox)
                    request.Headers.Add("User-agent", "RobloxProxy");
                else if (source != FileSource.Other)
                    request.Headers.Add("User-agent", $"SSQE_Import-{source}");

                using HttpResponseMessage response = await determined.SendAsync(request);
                using HttpContent content = response.Content;

                Stream stream = await content.ReadAsStreamAsync();

                Logging.Register($"Attempted download of file: {location} - {source} ({url}) : {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using FileStream fs = new(location, FileMode.Create);
                    await stream.CopyToAsync(fs);
                }

                return response.StatusCode;
            });

            if (result.Result != HttpStatusCode.OK)
                throw new WebException($"({(int)result.Result}) {result.Result}");
        }
    }
}
