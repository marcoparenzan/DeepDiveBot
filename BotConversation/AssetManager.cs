using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BotConversation
{
    public class AssetManager
    {
        private string path;
        private Dictionary<string, BotConfiguration> botConfigurations = new Dictionary<string, BotConfiguration>();
        private Dictionary<string, string> adaptiveCards = new Dictionary<string, string>();

        public AssetManager(string path) => this.path = Path.GetFullPath(path);
        
        public BotConfiguration BotConfiguration(string name)
        {
            //if (botConfigurations.ContainsKey(name)) return botConfigurations[name];
            var c = JsonConvert.DeserializeObject<BotConversation.BotConfiguration>(File.ReadAllText(Path.Combine(path, $"{name}.json")));
            //botConfigurations.Add(name, c);
            return c;
        }

        public string AdaptiveCards(string name)
        {
            //if (adaptiveCards.ContainsKey(name)) return adaptiveCards[name];
            var c = File.ReadAllText(Path.Combine(Path.Combine(path, "AdaptiveCards"), $"{name}.json"));
            //adaptiveCards.Add(name, c);
            return c;
        }
    }
}
