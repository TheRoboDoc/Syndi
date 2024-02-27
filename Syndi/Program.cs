using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Syndi
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        public static DiscordClient? BotClient { get; private set; }

        static async Task MainAsync()
        {
            LogLevel logLevel;

            if (DebugStatus())
            {
                logLevel = LogLevel.Debug;
            }
            else
            {
                logLevel = LogLevel.Information;
            }

            string token = Tokens.Discord;

            DiscordConfiguration config = new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,

                Intents = 
                    DiscordIntents.MessageContents |
                    DiscordIntents.Guilds |
                    DiscordIntents.GuildMessages |
                    DiscordIntents.GuildMembers,

                MinimumLogLevel = logLevel,
                LogUnknownEvents = DebugStatus(),

                LogTimestampFormat = "dd.MM.yyyy HH:mm:ss (zzz)"
            };

            BotClient = new DiscordClient(config);

            BotClient.SessionCreated += BotClientReady;

            await BotClient.ConnectAsync();

            BotClient.Logger.LogInformation(LoggerEvents.Startup, "Bot is now operational");

            await Task.Delay(-1);
        }

        public static bool DebugStatus()
        {
            bool debugState;

            if (Debugger.IsAttached)
            {
                debugState = true;
            }
            else
            {
                debugState = false;
            }

            return debugState;
        }

        private static async Task BotClientReady(DiscordClient sender, SessionReadyEventArgs args)
        {
            await Task.Run(() =>
            {
                BotClient?.Logger.LogInformation(LoggerEvents.Startup, "Client is ready");
            });
        }
    }
}
