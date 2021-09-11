using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.VoiceNext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace YAMBot.Commands
{    
    class VoiceChannelCommands : BaseCommandModule
    {
        [Command("join")]
        public async Task JoinCommand(CommandContext commandContext, DiscordChannel channel = null)
        {
            channel ??= commandContext.Member.VoiceState?.Channel;

            LavalinkExtension lava = commandContext.Client.GetLavalink();

            if(!lava.ConnectedNodes.Any())
            {
                await commandContext.RespondAsync("Lavalink not connected");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();

            if(channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await commandContext.RespondAsync($"Not a valid voice channel: {channel.Name}");
                return;
            }

            await node.ConnectAsync(channel);            
        }

        [Command("leave")]
        public async Task LeaveCommand(CommandContext commandContext, DiscordChannel channel = null)
        {
            channel ??= commandContext.Member.VoiceState?.Channel;
            LavalinkExtension lava = commandContext.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await commandContext.RespondAsync("Lavalink not connected");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await commandContext.RespondAsync($"Not a valid voice channel: {channel.Name}");
                return;
            }

            LavalinkGuildConnection conn = node.GetGuildConnection(channel.Guild);

            await conn?.DisconnectAsync();
        }

        [Command("play")]
        public async Task PlayCommand(CommandContext commandContext, [RemainingText] string search)
        {
            if(commandContext.Member.VoiceState == null || commandContext.Member.VoiceState.Channel == null)
            {
                await commandContext.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = commandContext.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(commandContext.Member.VoiceState.Guild);

            if(conn == null)
            {
                await commandContext.RespondAsync("Lavalink not connected");
                return;
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            switch(loadResult.LoadResultType)
            {
                case LavalinkLoadResultType.LoadFailed:
                case LavalinkLoadResultType.NoMatches:
                    await commandContext.RespondAsync($"Track search failed for {search}");
                    return;
                default:
                    break;
            }

            var track = loadResult.Tracks.First();
            await conn.PlayAsync(track);
        }
    }
}
