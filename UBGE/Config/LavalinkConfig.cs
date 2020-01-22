using DSharpPlus.CommandsNext;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace UBGE.Config
{
    public sealed class LavalinkConfig
    {
        [JsonProperty("LavalinkIP")]
        public string LavalinkIP { get; private set; } = "127.0.0.1";

        [JsonProperty("LavalinkPort")]
        public int LavalinkPort { get; private set; } = 2333;

        [JsonProperty("LavalinkPassword")]
        public string LavalinkPassword { get; private set; } = "youshallnotpass";

        public LavalinkConfig Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<LavalinkConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\LavalinkConfig.json"));

            return new LavalinkConfig
            {
                LavalinkIP = jsonConfig.LavalinkIP,
                LavalinkPort = jsonConfig.LavalinkPort,
                LavalinkPassword = jsonConfig.LavalinkPassword
            };
        }
    }
}
