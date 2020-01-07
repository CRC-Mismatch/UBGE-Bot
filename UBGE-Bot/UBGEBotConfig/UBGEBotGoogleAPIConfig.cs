﻿using Newtonsoft.Json;
using System;
using System.IO;

namespace UBGE_Bot.UBGEBotConfig
{
    public sealed class UBGEBotGoogleAPIConfig
    {
        [JsonProperty("google_censo_id")]
        public string censoID { get; set; }

        [JsonProperty("google_censo_range")]
        public string censoRange { get; set; }

        [JsonProperty("google_censoplanilha_range")]
        public string censoPlanilhaRange { get; set; }

        [JsonProperty("google_infracoes_id")]
        public string infracaoID { get; set; }

        [JsonProperty("google_infracoes_range")]
        public string infracaoRange { get; set; }

        [JsonProperty("CadastroGuildAlbionSpreadsheet")]
        public string cadastroGuildAlbionSpreadsheet { get; set; }

        [JsonProperty("CadastroGuildAlbionRange")]
        public string cadastroGuildAlbionRange { get; set; }

        public UBGEBotGoogleAPIConfig Build()
        {
            UBGEBotGoogleAPIConfig jsonConfig = JsonConvert.DeserializeObject<UBGEBotGoogleAPIConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\GoogleSheetsConfig.json"));

            return new UBGEBotGoogleAPIConfig
            {
                censoID = jsonConfig.censoID,
                censoRange = jsonConfig.censoRange,
                censoPlanilhaRange = jsonConfig.censoPlanilhaRange,
                infracaoID = jsonConfig.infracaoID,
                infracaoRange = jsonConfig.infracaoRange,
                cadastroGuildAlbionSpreadsheet = jsonConfig.cadastroGuildAlbionSpreadsheet,
                cadastroGuildAlbionRange = jsonConfig.cadastroGuildAlbionRange,
            };
        }
    }
}