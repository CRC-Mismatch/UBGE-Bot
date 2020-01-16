using Newtonsoft.Json;
using System;
using System.IO;

namespace UBGE.Config
{
    public sealed class GoogleAPIsConfig
    {
        [JsonProperty("Google_censo_id")]
        public string CensusID { get; set; }

        [JsonProperty("Google_censo_range")]
        public string CensusRange { get; set; }

        [JsonProperty("Google_censoplanilha_range")]
        public string CensusSheetRange { get; set; }

        [JsonProperty("Google_infracoes_id")]
        public string InfracaoID { get; set; }

        [JsonProperty("Google_infracoes_range")]
        public string InfracaoRange { get; set; }

        [JsonProperty("CadastroGuildAlbionSpreadsheet")]
        public string RegisterGuildAlbionSheet { get; set; }

        [JsonProperty("CadastroGuildAlbionRange")]
        public string RegisterGuildAlbionRange { get; set; }

        public GoogleAPIsConfig Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<GoogleAPIsConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\GoogleSheetsConfig.json"));

            return new GoogleAPIsConfig
            {
                CensusID = jsonConfig.CensusID,
                CensusRange = jsonConfig.CensusRange,
                CensusSheetRange = jsonConfig.CensusSheetRange,
                InfracaoID = jsonConfig.InfracaoID,
                InfracaoRange = jsonConfig.InfracaoRange,
                RegisterGuildAlbionSheet = jsonConfig.RegisterGuildAlbionSheet,
                RegisterGuildAlbionRange = jsonConfig.RegisterGuildAlbionRange,
            };
        }
    }
}