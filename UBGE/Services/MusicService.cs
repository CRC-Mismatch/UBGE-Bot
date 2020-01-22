using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UBGE.Entities.Music;
using Log = UBGE.Logger.Logger;

namespace UBGE.Services
{
    public sealed class MusicService
    {
        private ConnectionEndpoint ConnectionEndPoint { get; set; }
        private LavalinkConfiguration LavalinkConfig { get; set; }
        private LavalinkNodeConnection LavalinkNodeConnection { get; set; }
        private ConcurrentDictionary<ulong, MusicPlayer> MusicPlayers { get; set; }

        private UBGE_Bot Bot { get; set; }

        public MusicService(UBGE_Bot bot)
        {
            this.Bot = bot;

            this.MusicPlayers = new ConcurrentDictionary<ulong, MusicPlayer>();

            var lavalinkConfig = this.Bot.BotConfig.LavalinkConfig;
            this.ConnectionEndPoint = new ConnectionEndpoint(lavalinkConfig.LavalinkIP, lavalinkConfig.LavalinkPort);
            this.LavalinkConfig = new LavalinkConfiguration
            {
                Password = lavalinkConfig.LavalinkPassword,
                RestEndpoint = this.ConnectionEndPoint,
                SocketEndpoint = this.ConnectionEndPoint,
            };

            this.Bot.DiscordClient.Ready += this.Ready;
            this.Bot.DiscordClient.Heartbeated += this.Heartbeated;
        }

        public async Task ValidateNodeConnectionAsync()
        {
            try
            {
                if (this.LavalinkNodeConnection == null)
                    this.LavalinkNodeConnection = this.Bot.Lavalink.GetNodeConnection(this.ConnectionEndPoint);

                if (this.LavalinkNodeConnection == null)
                {
                    this.LavalinkNodeConnection = await this.Bot.Lavalink.ConnectAsync(this.LavalinkConfig);
                    this.LavalinkNodeConnection.Disconnected += this.NodeDisconnected;
                    this.LavalinkNodeConnection.LavalinkSocketErrored += this.NodeSocketError;
                }
            }
            catch (Exception)
            {
                this.Bot.Logger.Warning(Log.TypeWarning.Lavalink, "Não foi possível conectar ao Lavalink!");
            }
        }

        async Task NodeDisconnected(NodeDisconnectedEventArgs e)
        {
            await Task.Delay(0);

            this.MusicPlayers.Clear();
        }

        async Task NodeSocketError(SocketErrorEventArgs e)
        {
            await Task.Delay(0);

            this.MusicPlayers.Clear();
        }

        public Task<LavalinkLoadResult> GetTracksAsync(string music) => this.LavalinkNodeConnection.Rest.GetTracksAsync(music);

        public IMusicPlayer GetOrCreatePlayerAsync(DiscordGuild guild)
        {
            if (this.MusicPlayers.TryGetValue(guild.Id, out var p))
                return p;

            p = new MusicPlayer(this.LavalinkNodeConnection, guild);
            p.OnInitialized += e => this.Bot.Logger.Warning(Log.TypeWarning.Lavalink, $"Player de música inicializado no servidor: {e.Guild.Name}");

            p.OnShutdown += e =>
            {
                this.MusicPlayers.TryRemove(e.Guild.Id, out var _);

                this.Bot.Logger.Warning(Log.TypeWarning.Lavalink, $"Player de música fechado no servidor: {e.Guild.Name}");
            };

            return this.MusicPlayers.AddOrUpdate(guild.Id, p, (key, old) => p);
        }

        async Task Ready(ReadyEventArgs e) => await this.ValidateNodeConnectionAsync();

        async Task Heartbeated(HeartbeatEventArgs e) => await this.ValidateNodeConnectionAsync();
    }
}
