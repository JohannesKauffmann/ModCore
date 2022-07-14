﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using ModCore.Entities;
using ModCore.Extensions;
using ModCore.Utils.Extensions;
using ModCore.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace ModCore.SlashCommands
{
    public class Main : ApplicationCommandModule
    {
        public SharedData Shared { private get; set; }
        public StartTimes StartTimes { private get; set; }

        [SlashCommand("about", "Prints information about ModCore.")]
        public async Task AboutAsync(InteractionContext ctx)
        {
            var eb = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#089FDF"))
                .WithTitle("ModCore")
                .WithDescription("A powerful moderating bot written on top of DSharpPlus")
                .AddField("Main developer", "[Naamloos](https://github.com/Naamloos)")
                .AddField("Special thanks to these contributors:",
                    "[uwx](https://github.com/uwx), " +
                    "[jcryer](https://github.com/jcryer), " +
                    "[Emzi0767](https://github.com/Emzi0767), " +
                    "[YourAverageBlackGuy](https://github.com/YourAverageBlackGuy), " +
                    "[DrCreo](https://github.com/DrCreo), " +
                    "[aexolate](https://github.com/aexolate), " +
                    "[Drake103](https://github.com/Drake103) and " +
                    "[Izumemori](https://github.com/Izumemori)")
                .AddField("Environment",
                    $"*OS:* {Environment.OSVersion.VersionString}" +
                    $"\n*Framework:* {Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}" +
                    $"\n*DSharpPlus:* {ctx.Client.VersionString}" +
                    $"\n*Servers:* {this.Shared.ModCore.Shards.Select(x => x.Client.Guilds.Count).Sum()}" +
                    $"\n*Shards:* {this.Shared.ModCore.Shards.Count}")
                .AddField("Contribute?", "Contributions are always welcome at our [GitHub repo.](https://github.com/Naamloos/ModCore)")
                .WithThumbnail(ctx.Client.CurrentUser.AvatarUrl)
                .Build();

            await ctx.CreateResponseAsync(eb, true);
        }

        [SlashCommand("avatar", "Fetches a user's avatar with URL.")]
        public async Task AvatarAsync(InteractionContext ctx, [Option("user", "User to fetch the avatar from.")]DiscordUser user)
        {
            var img = user.GetAvatarUrl(ImageFormat.Png, 4096);

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Avatar for user {user.Username}")
                .WithDescription(img)
                .WithImageUrl(img);

            await ctx.CreateResponseAsync(embed, true);
        }

        [SlashCommand("status", "Returns ModCore status info.")]
        public async Task StatusAsync(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("ModCore Status")
                .WithDescription("Information about ModCore's status.")
                .WithThumbnail(ctx.Client.CurrentUser.GetAvatarUrl(ImageFormat.Png))
                .AddField("🏓 Socket Ping", $"{ctx.Client.Ping} ms", true)
                .AddField("⚡ Servers", $"{this.Shared.ModCore.Shards.Select(x => x.Client.Guilds.Count).Sum()}", true)
                .AddField("⚡ Shards", $"{ctx.Client.ShardCount}", true)
                .AddField("⚡ Current Shard", $"{ctx.Client.ShardId}", true)
                .AddField("⏱️ Program Uptime", string.Format("<t:{0}:R>", StartTimes.ProcessStartTime.ToUnixTimeSeconds()), true)
                .AddField("⏱️ Socket Uptime", string.Format("<t:{0}:R>", StartTimes.SocketStartTime.ToUnixTimeSeconds()), true);

            await ctx.CreateResponseAsync(embed, true);
        }

        [SlashCommand("snipe", "Retrieves the last deleted message from cache.")]
        public async Task SnipeAsync(InteractionContext ctx, [Option("edit", "Whether to fetch an edited message or a deleted one.")]bool edit = false)
        {
            var messages = edit ? this.Shared.EditedMessages : this.Shared.DeletedMessages;
            if (messages.ContainsKey(ctx.Channel.Id))
            {
                var message = this.Shared.DeletedMessages[ctx.Channel.Id];

                var content = message.Content;
                if (content.Length > 500)
                    content = content.Substring(0, 500) + "...";

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor($"{message.Author.Username}#{message.Author.Discriminator}" + (edit? " (Edited)" : ""), 
                        iconUrl: message.Author.GetAvatarUrl(ImageFormat.Png));

                if (!string.IsNullOrEmpty(message.Content))
                {
                    embed.WithDescription(message.Content);
                    embed.WithTimestamp(message.Id);
                }

                if (message.Attachments.Count > 0)
                {
                    if (message.Attachments[0].MediaType == "image/png"
                        || message.Attachments[0].MediaType == "image/jpeg"
                        || message.Attachments[0].MediaType == "image/gif"
                        || message.Attachments[0].MediaType == "image/apng"
                        || message.Attachments[0].MediaType == "image/webp")
                        embed.WithImageUrl(message.Attachments[0].Url);
                }

                await ctx.CreateResponseAsync(embed);
                return;
            }

            await ctx.CreateResponseAsync("⚠️ No message to snipe!", true);
        }

        [SlashCommand("invite", "Get an invite to this ModCore instance. Sharing is caring!")]
        public async Task InviteAsync(InteractionContext ctx)
        {
            var app = ctx.Client.CurrentApplication;
            if (app.IsPublic != null && (bool)app.IsPublic)
                await ctx.CreateResponseAsync(
                    $"🛡 Add ModCore to your server!\n<https://modcore.naamloos.dev/info/invite>", true);
            else
                await ctx.CreateResponseAsync("⚠️ I'm sorry Mario, but this instance of ModCore has been set to private!", true);
        }
    }
}
