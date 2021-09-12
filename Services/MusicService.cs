using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace YAMBot.Services
{
    public class MusicService
    {
        private class GuildMusicPlayer
        {
            private LavalinkGuildConnection _lavalinkGuildConnection;
            private ConcurrentQueue<LavalinkTrack> _queue;
            public bool IsPlaying => _lavalinkGuildConnection?.CurrentState.CurrentTrack != null;

            public GuildMusicPlayer(LavalinkGuildConnection guildConnection)
            {
                _lavalinkGuildConnection = guildConnection;
                _queue = new ConcurrentQueue<LavalinkTrack>();
                _lavalinkGuildConnection.PlaybackFinished += OnPlaybackFinished;
            }

            private async Task OnPlaybackFinished(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
            {
                await DequeueAndPlay();
            }

            private async Task DequeueAndPlay()
            {
                LavalinkTrack track;
                if (_queue.TryDequeue(out track))
                {
                    await _lavalinkGuildConnection.PlayAsync(track);
                }
            }

            public async Task DisconnectAsync()
            {
                await _lavalinkGuildConnection.DisconnectAsync();
            }

            public async Task Enqueue(LavalinkTrack track)
            {
                if(IsPlaying)
                {
                    _queue.Enqueue(track);
                }
                else
                {
                    await _lavalinkGuildConnection.PlayAsync(track);
                }
            }

            public async Task Stop()
            {
                await _lavalinkGuildConnection.StopAsync();
                _queue.Clear();
            }

            public async Task Skip()
            {
                await _lavalinkGuildConnection.StopAsync();
                await DequeueAndPlay();
            }
        }

        private ConcurrentDictionary<ulong, GuildMusicPlayer> _musicPlayers;


        public MusicService()
        {
            _musicPlayers = new ConcurrentDictionary<ulong, GuildMusicPlayer>();
        }

        public void Initialize(ulong discordGuildId, LavalinkGuildConnection guildConnection)
        {
            var gmp = new GuildMusicPlayer(guildConnection);
            if (!_musicPlayers.TryAdd(discordGuildId, gmp))
                throw new ArgumentException($"MusicPlayer already initialized for {discordGuildId}");
        }

        public async Task Dispose(ulong discordGuildId)
        {
            GuildMusicPlayer gmp;
            if (!_musicPlayers.TryRemove(discordGuildId, out gmp))
            {
                throw new ArgumentException($"Failed to remove MusicPlayer for {discordGuildId}");
            }

            await gmp.DisconnectAsync();
        }

        public async Task Enqueue(ulong discordGuildId, LavalinkTrack track)
        {
            GuildMusicPlayer gmp;
            if(!_musicPlayers.TryGetValue(discordGuildId, out gmp))
            {
                throw new ArgumentException($"Guild {discordGuildId} not initialized");
            }

            await gmp.Enqueue(track);
        }

        public async Task Stop(ulong discordGuildId)
        {
            GuildMusicPlayer gmp;
            if (!_musicPlayers.TryGetValue(discordGuildId, out gmp))
            {
                throw new ArgumentException($"Guild {discordGuildId} not initialized");
            }

            await gmp.Stop();
        }

        public async Task Skip(ulong discordGuildId)
        {
            GuildMusicPlayer gmp;
            if (!_musicPlayers.TryGetValue(discordGuildId, out gmp))
            {
                throw new ArgumentException($"Guild {discordGuildId} not initialized");
            }

            await gmp.Skip();
        }
    }
}
