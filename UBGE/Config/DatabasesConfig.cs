using Newtonsoft.Json;
using System;
using System.IO;

namespace UBGE.Config
{
    public sealed class DatabasesConfig
    {
        [JsonProperty("MongoDBIP")]
        public string MongoDBIP { get; private set; }

        [JsonProperty("MongoDBPorta")]
        public string MongoDBPort { get; private set; }

        [JsonProperty("MySQLIP")]
        public string MySQLIP { get; private set; }

        [JsonProperty("MySQLPorta")]
        public string MySQLPort { get; private set; }

        [JsonProperty("MySQLDatabase")]
        public string MySQLDatabase { get; private set; }

        [JsonProperty("MySQLTabela")]
        public string MySQLTable { get; private set; }

        [JsonProperty("MySQLUsuario")]
        public string MySQLUser { get; private set; }

        [JsonProperty("MySQLSenha")]
        public string MySQLPassword { get; private set; }

        public DatabasesConfig Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<DatabasesConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\DBConnection.json"));

            return new DatabasesConfig
            {
                MongoDBIP = jsonConfig.MongoDBIP,
                MongoDBPort = jsonConfig.MongoDBPort,
                MySQLIP = jsonConfig.MySQLIP,
                MySQLPort = jsonConfig.MySQLPort,
                MySQLDatabase = jsonConfig.MySQLDatabase,
                MySQLTable = jsonConfig.MySQLTable,
                MySQLUser = jsonConfig.MySQLUser,
                MySQLPassword = jsonConfig.MySQLPassword,
            };
        }
    }
}