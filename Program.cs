using DSharpPlus;
using DSharpPlus.VoiceNext;
using DSharpPlus.CommandsNext;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using YAMBot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace YAMBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()                
                .CreateLogger();            

            ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog();

            if (!configuration.GetSection("yambottoken").Exists())
            {
                return;
            }

            var discord = new DiscordClient(new DiscordConfiguration()
            { 
                Token = configuration.GetSection("yambottoken").Value,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                LoggerFactory = loggerFactory
            });

            CommandsNextExtension commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });

            commands.RegisterCommands<VoiceChannelCommands>();

            discord.UseVoiceNext(new VoiceNextConfiguration()
            {
                EnableIncoming = true,               
            });

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
