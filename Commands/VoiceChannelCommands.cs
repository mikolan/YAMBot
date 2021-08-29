using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Threading.Tasks;
using Serilog;

namespace YAMBot.Commands
{    
    class VoiceChannelCommands : BaseCommandModule
    {
        [Command("join")]
        public async Task JoinCommand(CommandContext commandContext, DiscordChannel channel = null)
        {
            channel ??= commandContext.Member.VoiceState?.Channel;
            var voiceNextConnection = await channel.ConnectAsync();
            voiceNextConnection.VoiceReceived += VoiceNextConnection_VoiceReceived;
            voiceNextConnection.UserSpeaking += VoiceNextConnection_UserSpeaking;
        }

        private async Task VoiceNextConnection_UserSpeaking(VoiceNextConnection sender, DSharpPlus.EventArgs.UserSpeakingEventArgs e)
        {
            Log.Debug($"UserSpeaking\tSSR: {e.SSRC}\tUser: {e.User.Id}");
        }

        private async Task VoiceNextConnection_VoiceReceived(VoiceNextConnection sender, DSharpPlus.VoiceNext.EventArgs.VoiceReceiveEventArgs e)
        {
            Log.Debug($"VoiceReceived\tSSR: {e.SSRC}\tDur: {e.AudioDuration}ms");
        }

        [Command("leave")]
        public async Task LeaveCommand(CommandContext commandContext)
        {
            VoiceNextConnection connection = commandContext.Client.GetVoiceNext().GetConnection(commandContext.Guild);
            connection.Disconnect();
        }
    }
}
