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

        /// <summary>
        ///     Discord client
        /// </summary>
        public static DiscordClient? BotClient { get; private set; }

        /// <summary>
        ///     Main Thread
        /// </summary>
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

        /// <summary>
        ///     Checks if the bot is running in a debug enviroment
        /// </summary>
        /// 
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <c>True</c>: In debug
        ///         </item>
        /// 
        ///         <item>
        ///             <c>False</c>: Not in debug
        ///         </item>
        ///     </list>
        /// </returns>
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

        /// <summary>
        ///     What happens once the client is ready
        /// </summary>
        /// 
        /// <param name="sender">
        ///     Client that triggered this task
        /// </param>
        /// 
        /// <param name="e">
        ///     Ready event arguments arguments
        /// </param>
        private static async Task BotClientReady(DiscordClient sender, SessionReadyEventArgs args)
        {
            await Task.Run(() =>
            {
                BotClient?.Logger.LogInformation(LoggerEvents.Startup, "Client is ready");
            });
        }
    }
}
