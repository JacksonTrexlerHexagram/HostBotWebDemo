using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SocketIOClient;
using Newtonsoft.Json;
using SocketIOClient.JsonSerializer;
using UnityEngine;

namespace Hexagram.Saga.Networking {
    public class WebSocket {

        private SocketIO _client;
        
        private struct WebSocketAck {
            [JsonProperty("status_code")]
            public int statusCode;

            public object body;

        }
        
        public WebSocket(string host) {
            _client = new SocketIO(host);
            NewtonSoftSerializer serializer = new NewtonSoftSerializer();
            _client.JsonSerializer = serializer;
        }

        public async Task ConnectAsync() {
            Debug.Log("Connecting to websocket...");
            await _client.ConnectAsync();
            Debug.Log("Connected.");
        }

        public async Task DisconnectAsync() {
            await _client.DisconnectAsync();
        }

        public void OnConnected(EventHandler callback) {
            _client.OnConnected += callback;
        }
        
        public void On(string eventName, Action<SocketIOResponse> callback) {
            _client.On(eventName, callback);
        }
        
        public async Task EmitAsync<T>(string eventName, T payload, CancellationToken ct, bool waitForAck = true) { 
            Debug.Log("Emitting key " + eventName + " and value " + JsonConvert.SerializeObject(payload) + "...");

            bool acked = false;
            WebSocketAck ack = new WebSocketAck();

            await _client.EmitAsync(eventName, ct, (response) => {
                if (waitForAck) {
                    ack = response.GetValue<WebSocketAck>();
                    acked = true;
                }
            }, payload);

            if (waitForAck) {
                await Util.WaitUntil(() => acked, ct);
                Util.ThrowException(ack.statusCode, ack.body?.ToString());
            }
        }
    }
    

    // Serializes with NewtonSoft
    public class NewtonSoftSerializer : IJsonSerializer
    {
        public Func<JsonSerializerSettings> JsonSerializerOptions { get; }

        public JsonSerializeResult Serialize(object[] data)
        {
            var converter = new ByteArrayConverter();
            var settings = GetOptions();
            settings.Converters.Add(converter);
            string json = JsonConvert.SerializeObject(data, settings);
            
            return new JsonSerializeResult
            {
                Json = json,
                Bytes = converter.Bytes
            };
        }

        public T Deserialize<T>(string json)
        {
            var settings = GetOptions();
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public T Deserialize<T>(string json, IList<byte[]> bytes)
        {
            var converter = new ByteArrayConverter();
            converter.Bytes.AddRange(bytes);
            var settings = GetOptions();
            settings.Converters.Add(converter);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        private JsonSerializerSettings GetOptions()
        {
            JsonSerializerSettings options = null;
            if (OptionsProvider != null)
            {
                options = OptionsProvider();
            }
            if (options == null)
            {
                options = new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
            }
            return options;
        }

        public Func<JsonSerializerSettings> OptionsProvider { get; set; }
    }
    
    class ByteArrayConverter : JsonConverter
    {
        public ByteArrayConverter()
        {
            Bytes = new List<byte[]>();
        }

        internal List<byte[]> Bytes { get; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, global::Newtonsoft.Json.JsonSerializer serializer)
        {
            byte[] bytes = null;
            if (reader.TokenType == JsonToken.StartObject)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.PropertyName && reader.Value?.ToString() == "_placeholder")
                {
                    reader.Read();
                    if (reader.TokenType == JsonToken.Boolean && (bool)reader.Value)
                    {
                        reader.Read();
                        if (reader.TokenType == JsonToken.PropertyName && reader.Value?.ToString() == "num")
                        {
                            reader.Read();
                            if (reader.Value != null)
                            {
                                if (int.TryParse(reader.Value.ToString(), out int num))
                                {
                                    bytes = Bytes[num];
                                    reader.Read();
                                }
                            }
                        }
                    }
                }
            }
            return bytes;
        }

        public override void WriteJson(JsonWriter writer, object value, global::Newtonsoft.Json.JsonSerializer serializer)
        {
            var source = value as byte[];
            Bytes.Add(source.ToArray());
            writer.WriteStartObject();
            writer.WritePropertyName("_placeholder");
            writer.WriteValue(true);
            writer.WritePropertyName("num");
            writer.WriteValue(Bytes.Count - 1);
            writer.WriteEndObject();
        }
    }
}