using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Http;

namespace Serilog.Sinks.Dynatrace
{
    class DynatraceHttpClient : IHttpClient, IDisposable
    {
        private readonly HttpClient client;

        public DynatraceHttpClient(string accessToken)
        {
            client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Token", accessToken);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
        {
            using (var content = new StreamContent(contentStream))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = Encoding.UTF8.WebName };
                return await client.PostAsync(requestUri, content).ConfigureAwait(false);
            }
        }

        public void Configure(IConfiguration configuration) { }

        public void Dispose() => client?.Dispose();
    }
}