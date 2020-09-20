using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Application.InternalBots.Clients
{
    public class SlackClient
    {
        private readonly IHttpClientFactory _clientFactory;

        public SlackClient(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task PostMessage(string webhookurl, object content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, webhookurl);

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));
            request.Content = new ByteArrayContent(buffer);

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to send slack message with error code " + response.StatusCode + ", message: " + response.Content.ReadAsStringAsync());
            }
        }
    }
}
