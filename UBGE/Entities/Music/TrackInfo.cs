using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace UBGE.Entities.Music
{
    public sealed class TrackInfo
    {
        public DiscordChannel Channel { get; private set; }
        public DiscordMember Member { get; private set; }
        public LavalinkTrack Track { get; private set; }

        public TrackInfo(DiscordChannel channel, DiscordMember member, LavalinkTrack track)
        {
            this.Channel = channel;
            this.Member = member;
            this.Track = track;
        }
    }
}
