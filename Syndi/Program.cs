using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Xml;
using static Syndi.FileManager;

namespace Syndi
{
    internal class Program
    {
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Discord client
        /// </summary>
        public static DiscordClient? BotClient { get; private set; }

        public static Dictionary<string, List<MessageItem>> Messages = [];

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

            DiscordConfiguration config = new()
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

            List<string> dirsMissing = [.. FileManager.DirCheck().Result];

            SlashCommandsExtension slashCommands = BotClient.UseSlashCommands();

            slashCommands.RegisterCommands<SlashCommand>(766478619513585675);
            slashCommands.RegisterCommands<SlashCommand>(1204085975463239750);

            //Logging missing directories
            if (dirsMissing.Count != 0)
            {
                string message = "Missing following directories:\n";

                foreach (string dirMissing in dirsMissing)
                {
                    string dirMissingText = char.ToUpper(dirMissing[0]) + dirMissing[1..];

                    message += $"\t\t\t\t\t\t\t\t{dirMissingText}\n";
                }

                BotClient.Logger.LogWarning(LoggerEvents.Startup, "{message}", message);
            }

            BotClient.SessionCreated += BotClientReady;

            BotClient.Heartbeated += (async (client, args) =>
            {
                Paths paths = new();

                FieldInfo? field = typeof(Paths).GetField("dataPath");

                if (field == null)
                {
                    return;
                }

                string? path = field.GetValue(paths)?.ToString();

                if (path == null)
                {
                    return;
                }

                DirectoryInfo dirInfo = new(path);

                if (dirInfo.GetDirectories().Length == 0)
                {
                    return;
                }

                foreach (DirectoryInfo guildDir in dirInfo.GetDirectories())
                {
                    foreach (DirectoryInfo channelDir in guildDir.GetDirectories())
                    {
                        ChannelSetting? settings = ReadChannelSettings(guildDir.Name, channelDir.Name);

                        if (settings == null)
                        {
                            return;
                        }

                        List<MessageItem> messages;

                        try
                        {
                            messages = Messages[guildDir.Name];
                        }
                        catch
                        {
                            messages = [];
                        }

                        foreach (string url in settings.Value.RSSLinks)
                        {
                            using XmlReader reader = XmlReader.Create(url);

                            SyndicationFeed feed = SyndicationFeed.Load(reader);

                            SyndicationItem post = feed.Items.First();

                            if (messages.Where(message => message.MessageID == post.Id && message.ChannelID == channelDir.Name).Any())
                            {
                                return;
                            }

                            DiscordEmbedBuilder embed = new()
                            {
                                Title = post.Title.Text,
                                Description = post.Summary.Text,
                                Color = DiscordColor.HotPink,
                                ImageUrl = feed.ImageUrl.ToString(),

                                Url = post.Links.First().Uri.ToString()
                            };

                            ulong.TryParse(channelDir.Name, out ulong channelID);

                            DiscordChannel channel = await BotClient.GetChannelAsync(channelID);

                            await BotClient.SendMessageAsync(channel, $"## [Lue lisää]({post.Links.First().Uri})", embed.Build());

                            MessageItem messageItem = new(channelDir.Name, post.Id);

                            try
                            {
                                Messages[guildDir.Name].Add(messageItem);
                            }
                            catch
                            {
                                Messages.Add(guildDir.Name, []);
                                Messages[guildDir.Name].Add(messageItem);
                            }
                        }
                    }
                }
            });

            await BotClient.ConnectAsync();

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
