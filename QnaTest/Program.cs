using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace QnaTest
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("EndpointKey", "3d2ad8e1-bf87-4476-9673-f4c80f4dfcae");

            var requestBody = new {
                question = "lego space"
            };
            var requestBodyJson = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(requestBodyJson);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync("https://mpqnademo.azurewebsites.net/qnamaker/knowledgebases/fdebed58-3849-4bb9-bbae-a2ac0498153b/generateAnswer", content);

            var contentResponseString = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<GeneratedAnswerResponse>(contentResponseString);
        }
    }

    class GeneratedAnswerResponse
    {
        [JsonProperty("answers")]
        public AnswerResponse[] Answers { get; set; }
    }

    public class AnswerResponse
    {
        [JsonProperty("questions")]
        public string[] Questions { get; set; }
        [JsonProperty("answer")]
        public string Answer { get; set; }
        [JsonProperty("score")]
        public decimal Score { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
    }
}
