using DSharpPlus.CommandsNext;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace UBGE_Bot.UBGEBotConfig
{
    public sealed class UBGEBotCommandsNextConfig
    {
        [JsonProperty("EnableDms")]
        public bool enableDms { get; set; }

        [JsonProperty("EnableMentionPrefix")]
        public bool enableMentionPrefix { get; set; }

        [JsonProperty("EnableDefaultHelp")]
        public bool enableDefaultHelp { get; set; }

        [JsonProperty("Prefix")]
        public List<string> prefixBot { get; set; }

        public CommandsNextConfiguration Build(IServiceProvider iserviceProvider)
        {
            var jsonConfig = JsonConvert.DeserializeObject<UBGEBotCommandsNextConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\CommandsNextConfig.json"));

            return new CommandsNextConfiguration
            {
                EnableDms = jsonConfig.enableDms,
                EnableMentionPrefix = jsonConfig.enableMentionPrefix,
                EnableDefaultHelp = jsonConfig.enableDefaultHelp,
                StringPrefixes = jsonConfig.prefixBot,
                Services = iserviceProvider,
            };
        }
    }
}