using Newtonsoft.Json;
using System.Collections.Generic;

namespace UBGEBot.UBGEBotConfig
{
    public sealed class UBGEBotConfig_
    {
        public UBGEBotDiscordConfig ubgeBotDiscordConfig { get; private set; } = new UBGEBotDiscordConfig();
        public UBGEBotCommandsNextConfig ubgeBotCommandsNextConfig { get; private set; } = new UBGEBotCommandsNextConfig();
        public UBGEBotInteractivityConfig ubgeBotInteractivityConfig { get; private set; } = new UBGEBotInteractivityConfig();
    }

    public sealed class DBConnectionConfig
    {
        [JsonProperty("MongoDB_IP")]
        public string MongoDB_IP { get; private set; } = "127.0.0.1";

        [JsonProperty("MongoDB_Porta")]
        public string MongoDB_Port { get; private set; } = "27017";

        [JsonProperty("MySQL_IP")]
        public string MySQL_IP { get; private set; } = "127.0.0.1";

        [JsonProperty("MySQL_Porta")]
        public string MySQL_Porta { get; private set; } = "3306";

        [JsonProperty("MySQL_Database")]
        public string MySQL_Database { get; private set; } = "Qualquer uma";

        [JsonProperty("MySQL_Tabela")]
        public string MySQL_Tabela { get; private set; } = "Já disse acima";

        [JsonProperty("MySQL_Usuário")]
        public string MySQL_Usuario { get; private set; } = "root";

        [JsonProperty("MySQL_Senha")]
        public string MySQL_Senha { get; private set; } = "1234";
    }

    public sealed class ServidoresConfig
    {
        [JsonProperty("NomeServidoresPR")]
        public List<string> NomeDosServidoresDePR { get; private set; }

        [JsonProperty("NomeServidorConan")]
        public List<string> NomeDoServidorDeConan { get; private set; }

        [JsonProperty("NomeServidorDayZ")]
        public List<string> NomeDoServidorDeDayZ { get; private set; }

        [JsonProperty("NomeServidorOpenSpades")]
        public List<string> NomeDoServidorDeOpenSpades { get; private set; }

        [JsonProperty("NomeServidorCounterStrike")]
        public List<string> NomeDoServidorDeCounterStrike { get; private set; }

        [JsonProperty("NomeServidorUnturned")]
        public List<string> NomeDoServidorDeUnturned { get; private set; }

        [JsonProperty("NomeServidorMordhau")]
        public List<string> NomeDoServidorDeMordhau { get; private set; }
    }

    public sealed class OpenSpadesServidores 
    {
        [JsonProperty("name")]
        public string NomeServidor { get; set; }

        [JsonProperty("identifier")]
        public string IP { get; set; }

        [JsonProperty("map")]
        public string Mapa { get; set; }

        [JsonProperty("game_mode")]
        public string ModoDeJogo { get; set; }

        [JsonProperty("country")]
        public string Pais { get; set; }

        [JsonProperty("players_current")]
        public int PlayersJogando { get; set; }

        [JsonProperty("players_max")]
        public int PlayersMaximo { get; set; }

        [JsonProperty("game_version")]
        public string VersaoDoJogo { get; set; }
    }

    public sealed class GoogleSheetsAPIConfig
    {
        [JsonProperty("google_censo_id")]
        public string CensoID { get; set; }

        [JsonProperty("google_censo_range")]
        public string CensoRange { get; set; }

        [JsonProperty("google_infracoes_id")]
        public string InfracaoID { get; set; }

        [JsonProperty("google_infracoes_range")]
        public string InfracaoRange { get; set; }

        [JsonProperty("DesbanSpreadsheet")]
        public string DesbanSpreadsheet { get; set; }

        [JsonProperty("DesbanRange")]
        public string DesbanRange { get; set; }

        [JsonProperty("GuardSpreadsheet")]
        public string GuardSpreadsheet { get; set; }

        [JsonProperty("GuardRange")]
        public string GuardRange { get; set; }

        [JsonProperty("ReputacaoSpreadsheet")]
        public string ReputacaoSpreadsheet { get; set; }

        [JsonProperty("ReputacaoRange")]
        public string ReputacaoRange { get; set; }

        [JsonProperty("CadastroGuildAlbionSpreadsheet")]
        public string CadastroGuildAlbionSpreadsheet { get; set; }

        [JsonProperty("CadastroGuildAlbionRange")]
        public string CadastroGuildAlbionRange { get; set; }
    }
}