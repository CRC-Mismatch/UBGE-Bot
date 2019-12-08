namespace UBGE_Bot.UBGEBotConfig
{
    public sealed class UBGEBotConfig_
    {
        public UBGEBotDiscordConfig ubgeBotDiscordConfig { get; private set; } = new UBGEBotDiscordConfig();
        public UBGEBotCommandsNextConfig ubgeBotCommandsNextConfig { get; private set; } = new UBGEBotCommandsNextConfig();
        public UBGEBotInteractivityConfig ubgeBotInteractivityConfig { get; private set; } = new UBGEBotInteractivityConfig();

        public UBGEBotDatabasesConfig ubgeBotDatabasesConfig { get; private set; } = new UBGEBotDatabasesConfig().Build();
        public UBGEBotGoogleAPIConfig ubgeBotGoogleAPIConfig { get; private set; } = new UBGEBotGoogleAPIConfig().Build();
        public UBGEBotServidoresConfig ubgeBotServidoresConfig { get; private set; } = new UBGEBotServidoresConfig().Build();

        public UBGEBotValores ubgeBotValoresConfig { get; set; } = new UBGEBotValores().Build();
    }
}