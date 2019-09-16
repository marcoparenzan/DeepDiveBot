using Newtonsoft.Json;
using System.Collections.Generic;

namespace BotConversation
{
    public class Configuration
    {
        [JsonProperty("apps")]
        public AppsConfiguration Apps { get; set; }
        [JsonProperty("intents")]
        public IntentsConfiguration Intents { get; set; }
        [JsonProperty("responses")]
        public ResponsesConfiguration Responses { get; set; }
        [JsonProperty("conversation")]
        public ConversationConfiguration Conversation { get; set; }
    }

    public class AppsConfiguration : Dictionary<string, AppConfig>
    {
    }

    public class AppConfig
    {
        [JsonProperty("key")]
        public string Key { get; set; }
    }

    public class IntentsConfiguration: Dictionary<string, IntentEntry>
    {
    }

    public class IntentEntry : Dictionary<string, string[]>
    {
    }

    public class ResponsesConfiguration : Dictionary<string, ResponseEntry>
    {
    }

    public class ResponseEntry : Dictionary<string, string>
    {
    }

    public class ConversationConfiguration : Dictionary<string, ConversationStateConfig>
    {
    }

    public class ConversationStateConfig : Dictionary<string, ConversationIntentConfig>
    {
    }

    public class ConversationIntentConfig
    {
        [JsonProperty("newstate")]
        public string NewState { get; set; }
        [JsonProperty("response")]
        public string Response { get; set; }
    }
}
