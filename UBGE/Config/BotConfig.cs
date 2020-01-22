namespace UBGE.Config
{
    public sealed class BotConfig
    {
        public DiscordConfig DiscordConfig { get; set; } = new DiscordConfig().Build();
        public CommandsNextConfig CommandsNextConfig { get; set; } = new CommandsNextConfig().Build();

        public DatabasesConfig DatabasesConfig { get; set; } = new DatabasesConfig().Build();
        public GoogleAPIsConfig GoogleAPIConfig { get; set; } = new GoogleAPIsConfig().Build();
        public ServersUBGEConfig ServidoresConfig { get; set; } = new ServersUBGEConfig().Build();

        public ValoresConfig ValoresConfig { get; set; } = new ValoresConfig().Build();

        public LavalinkConfig LavalinkConfig { get; set; } = new LavalinkConfig().Build();
    }
}