using DSharpPlus;
using DSharpPlus.Net.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;

namespace UBGE.Config
{
    public sealed class DiscordConfig
    {
        [JsonProperty("Token")]
        public string Token { get; set; }

        public DiscordConfig Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<DiscordConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\DiscordConfig.json"));

            return new DiscordConfig
            {
                Token = jsonConfig.Token,
            };
        }
    }
}