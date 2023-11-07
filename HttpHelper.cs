using System.Net;

namespace Splatoon3ics
{
    internal class HttpHelper
    {
        readonly HttpClient httpClient;

        public HttpHelper()
        {
            CookieContainer cookies = new();
            HttpClientHandler handler = new()
            {
                CookieContainer = cookies
            };
            httpClient = new HttpClient(handler);

            //httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.162 Safari/537.36");
        }

        public async Task<string> GetUrl(string url)
        {
            return await httpClient.GetStringAsync(url);
        }
    }
}
