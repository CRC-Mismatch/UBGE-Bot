using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using Newtonsoft.Json;
using System;
using System.IO;

namespace UBGE_Bot.UBGEBotConfig
{
    public class UBGEBotInteractivityConfig
    {
        [JsonProperty("PaginationBehaviour")]
        public PaginationBehaviour paginationBehaviour { get; set; }

        [JsonProperty("Timeout")]
        public TimeSpan timeout { get; set; }

        public InteractivityConfiguration Build()
        {
            var jsonConfig = JsonConvert.DeserializeObject<UBGEBotInteractivityConfig>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\JsonUBGE_Bot\InteractivityConfig.json"));

            return new InteractivityConfiguration
            {
                PaginationBehaviour = jsonConfig.paginationBehaviour,
                Timeout = jsonConfig.timeout,
            };
        }
    }
}