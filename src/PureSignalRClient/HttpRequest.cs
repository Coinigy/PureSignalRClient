using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PureSignalRClient
{
    internal class HttpRequest
    {
        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";

        public static async Task<string> Get(string uri, int timeout = 20000, bool autoRetry = false)
        {
            var tries = 0;
            RETRY:
            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(httpClientHandler) {Timeout = TimeSpan.FromMilliseconds(timeout)}
                    )
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                        using (var result = await client.GetAsync(uri))
                        {
                            result.EnsureSuccessStatusCode();
                            var contentArray = await result.Content.ReadAsByteArrayAsync();
                            return Encoding.UTF8.GetString(contentArray, 0, contentArray.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (tries >= 11 || !autoRetry)
                    throw new HttpRequestException(
                        $"http get request failed URL: {uri} Message: {ex.Message} Reason: unknown 404 unknown");

                tries++;
                Thread.Sleep(1000);
                goto RETRY;
            }
        }

        public static async Task<Stream> GetStream(string uri, int timeout = 20000, bool autoRetry = false)
        {
            var tries = 0;
            RETRY:
            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(httpClientHandler) {Timeout = TimeSpan.FromMilliseconds(timeout)}
                    )
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                        var res = await client.GetAsync(uri);
                        res.EnsureSuccessStatusCode();
                        return await res.Content.ReadAsStreamAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (tries >= 11 || !autoRetry)
                    throw new HttpRequestException(
                        $"http get stream request failed URL: {uri} Message: {ex.Message} Reason: unknown 404 unknown");

                tries++;
                Thread.Sleep(1000);
                goto RETRY;
            }
        }

        public static async Task<string> Post(string uri, string postVals, int timeout = 20000, bool autoRetry = false)
        {
            var tries = 0;
            RETRY:
            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(httpClientHandler) {Timeout = TimeSpan.FromMilliseconds(timeout)}
                    )
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                        using (var result = await client.PostAsync(uri, new StringContent(postVals)))
                        {
                            result.EnsureSuccessStatusCode();
                            var contentArray = await result.Content.ReadAsByteArrayAsync();
                            return Encoding.UTF8.GetString(contentArray, 0, contentArray.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (tries >= 11 || !autoRetry)
                    throw new HttpRequestException(
                        $"http post 1 request failed URL: {uri} Message: {ex.Message} Reason: unknown 404 unknown");

                tries++;
                Thread.Sleep(1000);
                goto RETRY;
            }
        }

        public static async Task<string> Post(string uri, Dictionary<string, object> postVals = null,
            int timeout = 20000, bool autoRetry = false)
        {
            var tries = 0;
            RETRY:
            try
            {
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(httpClientHandler) {Timeout = TimeSpan.FromMilliseconds(timeout)}
                    )
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                        var jsonContent = new StringContent(JsonConvert.SerializeObject(postVals), Encoding.UTF8,
                            "application/json");
                        using (
                            var result =
                                await
                                    client.PostAsync(uri, jsonContent))
                        {
                            result.EnsureSuccessStatusCode();
                            var contentArray = await result.Content.ReadAsByteArrayAsync();
                            return Encoding.UTF8.GetString(contentArray, 0, contentArray.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (tries >= 11 || !autoRetry)
                    throw new HttpRequestException(
                        $"http post 2 request failed URL: {uri} Message: {ex.Message} Reason: unknown 404 unknown");

                tries++;
                Thread.Sleep(1000);
                goto RETRY;
            }
        }
    }
}