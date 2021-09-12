using DSharpPlus.CommandsNext;
using System.Threading.Tasks;

namespace YAMBot.Utils
{
    public class ChatUtils
    {
        public static async Task DeleteCommandMessage(CommandContext commandContext)
            => await commandContext.Channel.DeleteMessageAsync(commandContext.Message);
    }
}
