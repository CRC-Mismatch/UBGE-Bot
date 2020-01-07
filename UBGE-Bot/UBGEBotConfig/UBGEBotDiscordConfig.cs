using DSharpPlus;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;

namespace UBGE_Bot.UBGEBotConfig
{
    public sealed class UBGEBotDiscordConfig
    {
        [JsonProperty("Token")]
        public string tokenBot { get; set; }

        [JsonProperty("AutoReconnect")]
        public bool autoReconnect { get; set; }

        [JsonProperty("HttpTimeout")]
        public TimeSpan httpTimeout { get; set; }

        [JsonProperty("LargeThreshold")]
        public int largeThreshold { get; set; }

        [JsonProperty("LogLevel")]
        public LogLevel logLevel { get; set; }

        [JsonProperty("ReconnectIndefinitely")]
        public bool reconnectIndefinitely { get; set; }

        [JsonProperty("MessageCacheSize")]
        public int messageCacheSize { get; set; }

        [JsonProperty("UseInternalLogHandler")]
        public bool useInternalLogHandler { get; set; }

        [JsonProperty("GatewayCompressionLevel")]
        public GatewayCompressionLevel gatewayCompressionLevel { get; set; } = GatewayCompressionLevel.Stream;

        public DiscordConfiguration Build()
        {
            UBGEBotDiscordConfig jsonConfig = JsonConvert.DeserializeObject<UBGEBotDiscordConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\DiscordConfig.json"));

            return new DiscordConfiguration
            {
                Token = jsonConfig.tokenBot,
                AutoReconnect = jsonConfig.autoReconnect,
                HttpTimeout = jsonConfig.httpTimeout,
                LargeThreshold = jsonConfig.largeThreshold,
                LogLevel = jsonConfig.logLevel,
                ReconnectIndefinitely = jsonConfig.reconnectIndefinitely,
                MessageCacheSize = jsonConfig.messageCacheSize,
                UseInternalLogHandler = jsonConfig.useInternalLogHandler,
                GatewayCompressionLevel = jsonConfig.gatewayCompressionLevel,
                WebSocketClientFactory = WebSocket4NetCoreClient.CreateNew
            };
        }
    }
}