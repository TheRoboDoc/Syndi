using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.ComponentModel;
using static Syndi.FileManager;

namespace Syndi
{
    internal class SlashCommand : ApplicationCommandModule
    {
        [SlashCommand("Ping", "Pings the bot, the bot responds with the ping time in milliseconds")]
        [SlashCommandPermissions(Permissions.SendMessages)]
        public static async Task Ping(InteractionContext ctx,
            [Option("Times", "Amount of times the bot should be pinged (Max 3)")]
            [DefaultValue(1)]
            [Minimum(1)]
            [Maximum(3)]
            double times = 1,

            [Option("Visible", "Is the ping visible to others")]
            [DefaultValue(false)]
            bool visible = false)
        {
            await ctx.CreateResponseAsync($"Pong {ctx.Client.Ping}ms", !visible);
            times--;

            for (int i = 0; times > i; times--)
            {
                DiscordFollowupMessageBuilder followUp = new()
                {
                    Content = $"Pong {ctx.Client.Ping}ms",
                    IsEphemeral = !visible
                };

                await ctx.FollowUpAsync(followUp);
            }
        }

        [SlashCommandGroup("RSS", "Set of RSS commands")]
        [SlashCommandPermissions(Permissions.Administrator)]
        public class RSS
        {
            [SlashCommand("Add", "Add an RSS post entry")]
            public static async Task Add(InteractionContext ctx,
                [Option("RSS_Link", "A link to the RSS feed")]
                string RSSLink,

                [Option("Visible", "Is the command visible to others")]
                [DefaultValue(false)]
                bool visible = false)
            {
                Permissions perms = ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember);

                bool hasPersm = perms.HasPermission(Permissions.SendMessages);

                if (!hasPersm)
                {
                    await ctx.CreateResponseAsync("I'm not allowed to post messages here", true);
                    return;
                }

                string guildID = ctx.Guild.Id.ToString();
                string channelID = ctx.Channel.Id.ToString();

                ChannelSetting? channelSetting = ReadChannelSettings(guildID, channelID);

                List<string> links = [];

                if (channelSetting == null)
                {
                    ChannelSetting newSettings = new()
                    {
                        RSSLinks = []
                    };

                    channelSetting = newSettings;
                }
                else
                {
                    channelSetting.Value.RSSLinks.ForEach(links.Add);
                }

                channelSetting.Value.RSSLinks?.Add(RSSLink);

                WriteChannelSettings(channelSetting.Value, guildID, channelID);

                await ctx.CreateResponseAsync($"Added {RSSLink} to {ctx.Channel.Mention}", !visible);
            }
        }
    }
}
