using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp.Services
{
    public class LuisNLP
    {
        IConfiguration config;
        private ApiKeyServiceClientCredentials credentials;
        private LUISRuntimeClient luisClient;
        private Prediction prediction;

        public LuisNLP(IConfiguration config)
        {
            this.config = config;
            this.credentials = new ApiKeyServiceClientCredentials(config["Luis.AuthoringKey"]);
            this.luisClient = new LUISRuntimeClient(credentials);
            this.luisClient.Endpoint = config["Luis.Endpoint"];
            this.prediction = new Prediction(luisClient);
        }

        public async Task<(string intent, (string name, string value)[] entities)> Resolve(string culture, string query)
        {
            var result = await prediction.ResolveAsync(config[$"Luis.App.{culture}"], query, verbose: true, cancellationToken: CancellationToken.None);
            var topScoringIntent = result.TopScoringIntent;
            if (topScoringIntent == null)
            {
                return ("None", null);
            }
            if (topScoringIntent.Score < 0.68)
            {
                return ("None", null);
            }
            var entities = result.Entities.Select(xx => (name: xx.Type, value: xx.Entity)).ToArray();
            return (topScoringIntent.Intent, entities);
        }
    }
}
