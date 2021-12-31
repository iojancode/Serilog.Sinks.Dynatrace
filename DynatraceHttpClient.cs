using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Token", accessToken);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content) 
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = Encoding.UTF8.WebName };
            return await client.PostAsync(requestUri, content);
        } 

        public void Dispose() => client?.Dispose();
    }
}