using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Threading.Tasks;
using Serilog;
using System.Collections.Concurrent;
using System.IO;
using DSharpPlus.VoiceNext.EventArgs;
using System;
using Microsoft.Toolkit.HighPerformance.Extensions;
using Microsoft.Toolkit.HighPerformance;
using System.Threading;

namespace YAMBot.Commands
{    
    class VoiceChannelCommands : BaseCommandModule
    {
        private ConcurrentDictionary<uint, ulong> _ssrcToUidMap;
        private ConcurrentDictionary<uint, MemoryStream> _ssrcToBuffer;
        private ConcurrentDictionary<uint, System.Timers.Timer> _ssrcToTimer;
        private TimeSpan _timeout = TimeSpan.FromMilliseconds(100);

        [Command("join")]
        public async Task JoinCommand(CommandContext commandContext, DiscordChannel channel = null)
        {
            channel ??= commandContext.Member.VoiceState?.Channel;
            var voiceNextConnection = await channel.ConnectAsync();
            _ssrcToUidMap = new ConcurrentDictionary<uint, ulong>();
            _ssrcToBuffer = new ConcurrentDictionary<uint, MemoryStream>();
            _ssrcToTimer = new ConcurrentDictionary<uint, System.Timers.Timer>();

            voiceNextConnection.VoiceReceived += OnVoiceReceived;
            voiceNextConnection.UserSpeaking += OnUserSpeaking;
            voiceNextConnection.UserJoined += OnUserJoined;
        }

        private Task OnUserJoined(VoiceNextConnection sender, VoiceUserJoinEventArgs e)
        {
            _ssrcToUidMap.TryAdd(e.SSRC, e.User.Id);
            return Task.CompletedTask;
        }

        private Task OnUserSpeaking(VoiceNextConnection sender, DSharpPlus.EventArgs.UserSpeakingEventArgs e)
        {
            _ssrcToUidMap.TryAdd(e.SSRC, e.User.Id);
            return Task.CompletedTask;
        }

        private async Task OnVoiceReceived(VoiceNextConnection sender, DSharpPlus.VoiceNext.EventArgs.VoiceReceiveEventArgs e)
        {
            if (!_ssrcToBuffer.ContainsKey(e.SSRC))
                _ssrcToBuffer[e.SSRC] = new MemoryStream();

            var ms = _ssrcToBuffer[e.SSRC];
            await e.PcmData.AsStream().CopyToAsync(ms);

            if (!_ssrcToTimer.ContainsKey(e.SSRC))
            {
                var timer = new System.Timers.Timer();
                timer.Interval = _timeout.TotalMilliseconds;
                timer.Elapsed += (sender, eventArgs) => FinishedSpeaking(sender, eventArgs, e.SSRC);
                timer.Enabled = true;
                timer.AutoReset = false;
                _ssrcToTimer[e.SSRC] = timer;
            }
            else
            {
                _ssrcToTimer[e.SSRC].Stop();
                _ssrcToTimer[e.SSRC].Start();
            }
        }

        private void FinishedSpeaking(object s, System.Timers.ElapsedEventArgs e, uint ssrc)
        {
            Log.Debug($"{ssrc} finished speaking");

            MemoryStream buffer;
            if (_ssrcToBuffer.TryRemove(ssrc, out buffer))
            {
                byte[] speechData = buffer.ToArray();
            }
            else
            {
                Log.Warning($"Failed to process buffer for SSRC: {ssrc}");
            }
        }

        [Command("leave")]
        public Task LeaveCommand(CommandContext commandContext)
        {
            VoiceNextConnection connection = commandContext.Client.GetVoiceNext().GetConnection(commandContext.Guild);
            connection.Disconnect();
            
            return Task.CompletedTask;
        }
    }
}
