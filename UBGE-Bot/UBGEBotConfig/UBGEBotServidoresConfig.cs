using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace UBGE_Bot.UBGEBotConfig
{
    public sealed class UBGEBotServidoresConfig
    {
        [JsonProperty("NomeServidoresPR")]
        public List<string> nomeDosServidoresDePR { get; private set; }

        [JsonProperty("NomeServidorConan")]
        public List<string> nomeDoServidorDeConan { get; private set; }

        [JsonProperty("NomeServidorDayZ")]
        public List<string> nomeDoServidorDeDayZ { get; private set; }

        [JsonProperty("NomeServidorOpenSpades")]
        public List<string> nomeDoServidorDeOpenSpades { get; private set; }

        [JsonProperty("NomeServidorCounterStrike")]
        public List<string> nomeDoServidorDeCounterStrike { get; private set; }

        [JsonProperty("NomeServidorUnturned")]
        public List<string> nomeDoServidorDeUnturned { get; private set; }

        [JsonProperty("NomeServidorMordhau")]
        public List<string> nomeDoServidorDeMordhau { get; private set; }

        public UBGEBotServidoresConfig Build()
        {
            UBGEBotServidoresConfig jsonConfig = JsonConvert.DeserializeObject<UBGEBotServidoresConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\ServidoresConfig.json"));

            return new UBGEBotServidoresConfig
            {
                nomeDosServidoresDePR = jsonConfig.nomeDosServidoresDePR,
                nomeDoServidorDeConan = jsonConfig.nomeDoServidorDeConan,
                nomeDoServidorDeDayZ = jsonConfig.nomeDoServidorDeDayZ,
                nomeDoServidorDeOpenSpades = jsonConfig.nomeDoServidorDeOpenSpades,
                nomeDoServidorDeCounterStrike = jsonConfig.nomeDoServidorDeCounterStrike,
                nomeDoServidorDeUnturned = jsonConfig.nomeDoServidorDeUnturned,
                nomeDoServidorDeMordhau = jsonConfig.nomeDoServidorDeMordhau,
            };
        }
    }
}