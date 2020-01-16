using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace UBGE.Config
{
    public sealed class ServersUBGEConfig
    {
        [JsonProperty("NomeServidoresPR")]
        public List<string> ServerPRName { get; private set; }

        [JsonProperty("NomeServidorConan")]
        public List<string> ServerConanName { get; private set; }

        [JsonProperty("NomeServidorDayZ")]
        public List<string> ServerDayZName { get; private set; }

        [JsonProperty("NomeServidorOpenSpades")]
        public List<string> ServerOpenSpadesName { get; private set; }

        [JsonProperty("NomeServidorCounterStrike")]
        public List<string> ServerCounterStrikeName { get; private set; }

        [JsonProperty("NomeServidorUnturned")]
        public List<string> ServerUnturnedName { get; private set; }

        [JsonProperty("NomeServidorMordhau")]
        public List<string> ServerMordhauName { get; private set; }

        public ServersUBGEConfig Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<ServersUBGEConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\ServidoresConfig.json"));

            return new ServersUBGEConfig
            {
                ServerPRName = jsonConfig.ServerPRName,
                ServerConanName = jsonConfig.ServerConanName,
                ServerDayZName = jsonConfig.ServerDayZName,
                ServerOpenSpadesName = jsonConfig.ServerOpenSpadesName,
                ServerCounterStrikeName = jsonConfig.ServerCounterStrikeName,
                ServerUnturnedName = jsonConfig.ServerUnturnedName,
                ServerMordhauName = jsonConfig.ServerMordhauName,
            };
        }
    }
}