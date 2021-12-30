using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Serilog.Sinks.Http;

namespace Serilog.Sinks.Dynatrace
{
    class DynatraceHttpClient : IHttpClient
    {
        private readonly HttpClient client;

        public DynatraceHttpClient(string accessToken)
        {
            client = new HttpClient();

            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Token", accessToken);
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content) => client.PostAsync(requestUri, content);

        public void Dispose() => client?.Dispose();
    }
}