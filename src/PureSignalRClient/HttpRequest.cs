using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PureSignalR
{
    internal static class HttpRequest
    {
        private const string UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

        public static async Task<string> Get(string uri, int timeout = 20000, bool ignoreCertErrors = false, bool autoRetry = false)
        {
            var tries = 0;
            RETRY:
            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    if(ignoreCertErrors)
                        httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(httpClientHandler) {Timeout = TimeSpan.FromMilliseconds(timeout)}
                    )
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                        using (var result = await client.GetAsync(uri))
                        {
                            result.EnsureSuccessStatusCode();
							return await result.Content.ReadAsStringAsync();
						}
                    }
                }
            }
            catch (Exception ex)
            {
                if (tries >= 11 || !autoRetry)
                    throw new HttpRequestException($"http get request failed URL: {uri} Message: {ex.Message} Reason: unknown 404 unknown");

                tries++;
                Thread.Sleep(1000);
                goto RETRY;
            }
        }
    }
}