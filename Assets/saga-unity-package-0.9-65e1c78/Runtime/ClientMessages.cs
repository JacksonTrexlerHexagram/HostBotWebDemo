using Newtonsoft.Json;

namespace Hexagram.Saga {
    public partial class Client {
        
        public class SagaMessage {
            [JsonProperty("user_id")]
            public string userId;
            [JsonProperty("bot_id")]
            public string botId;
            [JsonProperty("from", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public string from;
            [JsonProperty("body")]
            public string message;
        }
    }
}