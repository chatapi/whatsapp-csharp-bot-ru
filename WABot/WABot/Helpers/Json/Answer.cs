using Newtonsoft.Json;

namespace WABot.Helpers.Json
{
    /// <summary>
    /// Сериализованный класс из JSON тела, получаемого от chat-api
    /// </summary>
    public partial class Answer
    {
        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty("messages")]
        public Message[] Messages { get; set; }
    }

    public partial class Message
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("senderName")]
        public string SenderName { get; set; }

        [JsonProperty("fromMe")]
        public bool FromMe { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("chatId")]
        public string ChatId { get; set; }

        [JsonProperty("messageNumber")]
        public long MessageNumber { get; set; }
    }
}
