using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace UBGE.Entities.Music
{
    public interface IMusicPlayer
    {
        TrackInfo NowPlaying { get; }
        IReadOnlyList<TrackInfo> Tracks { get; }

        event Action<MusicPlayer> OnInitialized;
        event Action<MusicPlayer> OnShutdown;

        Task InitializeAsync(DiscordChannel chn);
        Task ShutdownAsync();

        void Enqueue(TrackInfo track);
        TrackInfo Dequeue();

        TimeSpan GetCurrentPlaybackPosition();
        void Seek(TimeSpan position, SeekMode mode = SeekMode.Current);

        void Play();
        void Pause();
        void Resume();
        void Stop();
    }
}
