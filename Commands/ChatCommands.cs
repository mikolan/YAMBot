using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace YAMBot.Commands
{
    class ChatCommands : BaseCommandModule
    {
        [Command("clear")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages, true)]
        public async Task ClearCommand(CommandContext commandContext)
        {
            var channel = commandContext.Channel;

            IReadOnlyList<DSharpPlus.Entities.DiscordMessage> toDelete;
            do
            {
                toDelete = await channel.GetMessagesAsync();
                foreach(var message in toDelete)
                {
                    await channel.DeleteMessageAsync(message, $"Deleted by: {commandContext.User.Username}");
                    await Task.Delay(10);
                }
            }
            while (toDelete.Count > 0);
        }

        [Command("clear")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages, true)]
        public async Task ClearCommand(CommandContext commandContext, int limit)
        {
            var channel = commandContext.Channel;
            var toDelete = await channel.GetMessagesAsync(limit);
            await channel.DeleteMessagesAsync(toDelete, $"Deleted by: {commandContext.User.Username}");
        }
    }
}
