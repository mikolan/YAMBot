using DSharpPlus;
using DSharpPlus.CommandsNext;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using YAMBot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;

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
                Token = configuration.GetValue<string>("yambottoken"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                LoggerFactory = loggerFactory
            });

            CommandsNextExtension commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });

            commands.RegisterCommands<VoiceChannelCommands>();

            var lavalinkEndpoint = new ConnectionEndpoint()
            {
                Hostname = configuration.GetValue<string>("lavalinkhostname"),
                Port = configuration.GetValue<int>("lavalinkport")
            };

            var lavalinkConfig = new LavalinkConfiguration()
            {
                Password = configuration.GetValue<string>("lavalinkpassword"),
                RestEndpoint = lavalinkEndpoint,
                SocketEndpoint = lavalinkEndpoint
            };

            LavalinkExtension lavalink = discord.UseLavalink();            

            await discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            await Task.Delay(-1);
        }
    }
}
