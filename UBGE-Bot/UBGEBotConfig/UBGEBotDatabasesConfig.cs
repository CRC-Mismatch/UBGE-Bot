using Newtonsoft.Json;
using System;
using System.IO;

namespace UBGE_Bot.UBGEBotConfig
{
    public sealed class UBGEBotDatabasesConfig
    {
        [JsonProperty("MongoDBIP")]
        public string mongoDBIP { get; private set; }

        [JsonProperty("MongoDBPorta")]
        public string mongoDBPorta { get; private set; }

        [JsonProperty("MySQLIP")]
        public string mySQLIP { get; private set; }

        [JsonProperty("MySQLPorta")]
        public string mySQLPorta { get; private set; }

        [JsonProperty("MySQLDatabase")]
        public string mySQLDatabase { get; private set; }

        [JsonProperty("MySQLTabela")]
        public string mySQLTabela { get; private set; }

        [JsonProperty("MySQLUsuario")]
        public string mySQLUsuario { get; private set; }

        [JsonProperty("MySQLSenha")]
        public string mySQLSenha { get; private set; }

        public UBGEBotDatabasesConfig Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<UBGEBotDatabasesConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\DBConnection.json"));

            return new UBGEBotDatabasesConfig
            {
                mongoDBIP = jsonConfig.mongoDBIP,
                mongoDBPorta = jsonConfig.mongoDBPorta,
                mySQLIP = jsonConfig.mySQLIP,
                mySQLPorta = jsonConfig.mySQLPorta,
                mySQLDatabase = jsonConfig.mySQLDatabase,
                mySQLTabela = jsonConfig.mySQLTabela,
                mySQLUsuario = jsonConfig.mySQLUsuario,
                mySQLSenha = jsonConfig.mySQLSenha,
            };
        }
    }
}