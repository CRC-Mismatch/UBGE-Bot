using DSharpPlus.CommandsNext;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace UBGE.Config
{
    public sealed class CommandsNextConfig
    {
        [JsonProperty("Prefix")]
        public List<string> Prefix { get; set; }

        public CommandsNextConfig Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<CommandsNextConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\CommandsNextConfig.json"));

            return new CommandsNextConfig
            {
                Prefix = jsonConfig.Prefix,
            };
        }
    }
}