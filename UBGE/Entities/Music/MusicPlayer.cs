using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace UBGE.Entities.Music
{
    public sealed class MusicPlayer : IMusicPlayer
    {
        private LavalinkNodeConnection LavalinkNodeConnection { get; set; }
        private LavalinkGuildConnection LavalinkGuildConnection { get; set; }

        private List<TrackInfo> InternalTracks { get; set; }
        public TrackInfo NowPlaying { get; private set; }

        public DiscordGuild Guild { get; private set; }

        public IReadOnlyList<TrackInfo> Tracks 
        {
            get
            {
                lock (this.InternalTracks)
                    return this.InternalTracks.AsReadOnly();
            }
        }

        public event Action<MusicPlayer> OnInitialized;
        public event Action<MusicPlayer> OnShutdown;

        public MusicPlayer(LavalinkNodeConnection lavalinkNodeConnection, DiscordGuild guild)
        {
            this.LavalinkNodeConnection = lavalinkNodeConnection;
            this.Guild = guild;
            this.LavalinkGuildConnection = default;
            this.InternalTracks = new List<TrackInfo>();
        }

        public async Task InitializeAsync(DiscordChannel channel)
        {
            if (this.LavalinkGuildConnection != null && this.LavalinkGuildConnection.IsConnected)
                return;

            this.LavalinkGuildConnection = await this.LavalinkNodeConnection.ConnectAsync(channel);
            this.LavalinkNodeConnection.PlaybackFinished += this.PlaybackFinished;

            try
            {
                this.LavalinkGuildConnection.SetVolume(65);
            }
            catch (Exception) { }

            this.OnInitialized?.Invoke(this);
        }

        async Task PlaybackFinished(TrackFinishEventArgs e) => await this.NotifyNextTrackAsync();
        
        public void ShutdownAsync()
        {
            if (this.LavalinkGuildConnection == null)
                return;

            if (this.LavalinkGuildConnection.IsConnected)
                this.LavalinkGuildConnection.Disconnect();

            this.LavalinkGuildConnection = null;

            this.OnShutdown?.Invoke(this);
        }

        public void Enqueue(TrackInfo track)
        {
            lock (this.InternalTracks)
                this.InternalTracks.Add(track);
        }

        public TrackInfo Dequeue()
        {
            lock (this.Tracks)
            {
                if (this.Tracks.Count == 0)
                    return null;

                var result = this.Tracks.FirstOrDefault();
                this.InternalTracks.RemoveAt(0);

                return result;
            }
        }

        internal async Task NotifyNextTrackAsync()
        {
            var np = this.Dequeue();

            if (np == null)
            {
                if (this.NowPlaying != null)
                    await this.NotifyPlaybackEnd(this.NowPlaying);

                this.ShutdownAsync();
            }
            else
            {
                this.NowPlaying = np;
                await this.NotifyTrackChangedAsync(this.NowPlaying);
                this.LavalinkGuildConnection.Play(this.NowPlaying.Track);
            }
        }

        internal async Task NotifyPlaybackEnd(TrackInfo ti)
        {
            if (ti == null)
                return;

            try
            {
                await ti.Channel.SendMessageAsync($"A playlist foi finalizada.");
            }
            catch (Exception) { }
        }

        public TimeSpan GetCurrentPlaybackPosition()
        {
            if (this.LavalinkGuildConnection == null || !this.LavalinkGuildConnection.IsConnected)
                return TimeSpan.Zero;

            return this.LavalinkGuildConnection.CurrentState.PlaybackPosition;
        }

        internal async Task NotifyTrackChangedAsync(TrackInfo trackInfo)
        {
            try
            {
                string msg = $"Tocando agora: {Formatter.Sanitize(trackInfo.Track.Title)} - {trackInfo.Track.Length.ToString(@"m\:ss")}, pedido por: {Formatter.Sanitize(trackInfo.Member.Username)}#{trackInfo.Member.Discriminator}";

                await trackInfo.Channel.SendMessageAsync(msg);
            }
            catch (Exception) {  }
        }

        public void Seek(TimeSpan position, SeekMode mode = SeekMode.Current)
        {
            if (this.LavalinkGuildConnection == null || !this.LavalinkGuildConnection.IsConnected)
                return;

            var offset = (mode == SeekMode.Current) ? (this.GetCurrentPlaybackPosition() + position) : position;
            this.LavalinkGuildConnection.Seek(offset);
        }

        public void Play()
        {
            if (this.LavalinkGuildConnection == null || !this.LavalinkGuildConnection.IsConnected)
                return;

            if (this.NowPlaying != null)
                return;

            this.NotifyNextTrackAsync().GetAwaiter().GetResult();
        }

        public void Pause()
        {
            if (this.LavalinkGuildConnection == null || !this.LavalinkGuildConnection.IsConnected)
                return;

            this.LavalinkGuildConnection.Pause();
        }

        public void Resume()
        {
            if (this.LavalinkGuildConnection == null || !this.LavalinkGuildConnection.IsConnected)
                return;

            this.LavalinkGuildConnection.Resume();
        }

        public void Stop()
        {
            if (this.LavalinkGuildConnection == null || !this.LavalinkGuildConnection.IsConnected)
                return;

            this.LavalinkGuildConnection.Stop();
        }

        Task IMusicPlayer.ShutdownAsync() => throw new NotImplementedException();
    }
}
