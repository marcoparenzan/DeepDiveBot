using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BotWpfClient.CognitiveServices
{
    public class TextToSpeechClient
    {
        private IConfiguration config;
        private string accessToken;

        public TextToSpeechClient(IConfiguration configuration)
        {
            this.config = configuration;
        }

        // https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support
        public async Task<byte[]> ToByteArray(string text, string locale)
        {
            async Task authorize()
            {
                var auth = new Authentication($"https://{config["SpeechToText.SpeechRegion"]}.api.cognitive.microsoft.com/sts/v1.0/issueToken", config["SpeechToText.SubscriptionKey"]);
                this.accessToken = await auth.FetchTokenAsync().ConfigureAwait(false);
            }

            if (string.IsNullOrWhiteSpace(this.accessToken))
            {
                await authorize();
            }

            var host = $"https://{config["SpeechToText.SpeechRegion"]}.tts.speech.microsoft.com/cognitiveservices/v1";

            var body = @"<speak version='1.0' xmlns='https://www.w3.org/2001/10/synthesis' xml:lang='" + locale + "'>" +
              $"<voice name='{config[$"TextToSpeech.VoiceName.{locale}"]}' xml:lang='{locale}' xml:gender='{config["TextToSpeech.Gender"]}'>{text}</voice></speak>";
            byte[] bytes = default;
            var failed = false;
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    // Set the HTTP method
                    request.Method = HttpMethod.Post;
                    // Construct the URI
                    request.RequestUri = new Uri(host);
                    // Set the content type header
                    request.Content = new StringContent(body, Encoding.UTF8, "application/ssml+xml");
                    // Set additional header, such as Authorization and User-Agent
                    request.Headers.Add("Authorization", "Bearer " + accessToken);
                    request.Headers.Add("Connection", "Keep-Alive");
                    // Update your resource name
                    request.Headers.Add("User-Agent", "InsightVirtualAssistant");
                    request.Headers.Add("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm");
                    // Create a request

                    HttpResponseMessage response = null;
                    try // can fail
                    {
                        response = await client.SendAsync(request).ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();

                        // Asynchronously read the response
                        bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        failed = true;
                        this.accessToken = null;
                    }
                }
            }
            if (failed) return await ToByteArray(text, locale);
            return (bytes);
        }

        public class Authentication
        {
            private string subscriptionKey;
            private string tokenFetchUri;

            public Authentication(string tokenFetchUri, string subscriptionKey)
            {
                if (string.IsNullOrWhiteSpace(tokenFetchUri))
                {
                    throw new ArgumentNullException(nameof(tokenFetchUri));
                }
                if (string.IsNullOrWhiteSpace(subscriptionKey))
                {
                    throw new ArgumentNullException(nameof(subscriptionKey));
                }
                this.tokenFetchUri = tokenFetchUri;
                this.subscriptionKey = subscriptionKey;
            }

            public async Task<string> FetchTokenAsync()
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.subscriptionKey);
                    UriBuilder uriBuilder = new UriBuilder(this.tokenFetchUri);

                    var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null).ConfigureAwait(false);
                    return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
