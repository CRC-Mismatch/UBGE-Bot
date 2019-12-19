namespace UBGE_Bot.UBGEBotConfig
{
    public sealed class UBGEBotConfig_
    {
        public UBGEBotDiscordConfig ubgeBotDiscordConfig { get; set; } = new UBGEBotDiscordConfig();
        public UBGEBotCommandsNextConfig ubgeBotCommandsNextConfig { get; set; } = new UBGEBotCommandsNextConfig();
        public UBGEBotInteractivityConfig ubgeBotInteractivityConfig { get; set; } = new UBGEBotInteractivityConfig();

        public UBGEBotDatabasesConfig ubgeBotDatabasesConfig { get; set; } = new UBGEBotDatabasesConfig();
        public UBGEBotGoogleAPIConfig ubgeBotGoogleAPIConfig { get; set; } = new UBGEBotGoogleAPIConfig();
        public UBGEBotServidoresConfig ubgeBotServidoresConfig { get; set; } = new UBGEBotServidoresConfig();

        public UBGEBotValores ubgeBotValoresConfig { get; set; } = new UBGEBotValores();
    }
}