using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.VoiceNext;
using System;
using System.Linq;
using System.Threading.Tasks;
using YAMBot.Services;
using YAMBot.Utils;

namespace YAMBot.Commands
{    
    class MusicCommands : BaseCommandModule
    {

        public MusicService MusicService { private get; set; }

        [Command("join")]
        public async Task JoinCommand(CommandContext commandContext, DiscordChannel channel = null)
        {
            await ChatUtils.DeleteCommandMessage(commandContext);
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
            var lavalinkConnection = await node.ConnectAsync(channel);
            MusicService.Initialize(commandContext.Guild.Id, lavalinkConnection);
        }

        [Command("leave")]
        public async Task LeaveCommand(CommandContext commandContext, DiscordChannel channel = null)
        {
            await ChatUtils.DeleteCommandMessage(commandContext);
            channel ??= commandContext.Member.VoiceState?.Channel;

            await MusicService.Dispose(channel.Guild.Id);
        }

        [Command("play")]
        public async Task PlayCommand(CommandContext commandContext, [RemainingText] string search)
        {
            await ChatUtils.DeleteCommandMessage(commandContext);
            if (commandContext.Member.VoiceState == null || commandContext.Member.VoiceState.Channel == null)
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

            await MusicService.Enqueue(commandContext.Member.VoiceState.Guild.Id, track);
        }

        [Command("stop")]
        public async Task StopCommand(CommandContext commandContext)
        {
            await ChatUtils.DeleteCommandMessage(commandContext);
            await MusicService.Stop(commandContext.Guild.Id);
        }

        [Command("skip")]
        public async Task SkipCommand(CommandContext commandContext)
        {
            await ChatUtils.DeleteCommandMessage(commandContext);
            await MusicService.Skip(commandContext.Guild.Id);
        }
    }
}
