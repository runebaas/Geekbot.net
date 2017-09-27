﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Geekbot.net.Lib;
using Serilog;
using StackExchange.Redis;

namespace Geekbot.net.Modules
{
    public class UserInfo : ModuleBase
    {
        private readonly IDatabase redis;
        private readonly IErrorHandler errorHandler;
        private readonly ILogger logger;
        
        public UserInfo(IDatabase redis, IErrorHandler errorHandler, ILogger logger)
        {
            this.redis = redis;
            this.errorHandler = errorHandler;
            this.logger = logger;
        }

        [Command("stats", RunMode = RunMode.Async)]
        [Summary("Get information about this user")]
        public async Task User([Summary("@someone")] IUser user = null)
        {
            var userInfo = user ?? Context.Message.Author;

            var age = Math.Floor((DateTime.Now - userInfo.CreatedAt).TotalDays);

            var messages = (int) redis.HashGet($"{Context.Guild.Id}:Messages", userInfo.Id.ToString());
            var level = LevelCalc.GetLevelAtExperience(messages);

            var guildKey = Context.Guild.Id.ToString();
            var guildMessages = (int) redis.HashGet($"{Context.Guild.Id}:Messages", 0.ToString());

            var percent = Math.Round((double) (100 * messages) / guildMessages, 2);

            var eb = new EmbedBuilder();
            eb.WithAuthor(new EmbedAuthorBuilder()
                .WithIconUrl(userInfo.GetAvatarUrl())
                .WithName(userInfo.Username));

            eb.WithColor(new Color(221, 255, 119));

            eb.AddField("Discordian Since",
                $"{userInfo.CreatedAt.Day}/{userInfo.CreatedAt.Month}/{userInfo.CreatedAt.Year} ({age} days)");
            eb.AddInlineField("Level", level)
                .AddInlineField("Messages Sent", messages)
                .AddInlineField("Server Total", $"{percent}%");

            var karma = redis.HashGet($"{Context.Guild.Id}:Karma", userInfo.Id.ToString());
            if (!karma.IsNullOrEmpty)
                eb.AddInlineField("Karma", karma);

            var correctRolls = redis.HashGet($"{Context.Guild.Id}:Rolls", userInfo.Id.ToString());
            if (!correctRolls.IsNullOrEmpty)
                eb.AddInlineField("Guessed Rolls", correctRolls);

            await ReplyAsync("", false, eb.Build());
        }

        [Command("rank", RunMode = RunMode.Async)]
        [Summary("get user top 10")]
        public async Task Rank()
        {
            try
            {
                var messageList = redis.HashGetAll($"{Context.Guild.Id}:Messages");
                var sortedList = messageList.OrderByDescending(e => e.Value).ToList();
                var guildMessages = (int) sortedList.First().Value;
                sortedList.RemoveAt(0);

                var highscoreUsers = new Dictionary<IGuildUser, int>();
                var listLimiter = 1;
                foreach (var user in sortedList)
                {
                    if (listLimiter > 10) break;
                    
                    var guildUser = Context.Guild.GetUserAsync((ulong) user.Name).Result;
                    if (guildUser != null)
                    {
                        highscoreUsers.Add(guildUser, (int)user.Value);
                        listLimiter++;
                    }
                }
                
                var highScore = new StringBuilder();
                highScore.AppendLine($":bar_chart: **Highscore for {Context.Guild.Name}**");
                var highscorePlace = 1;
                foreach (var user in highscoreUsers)
                {
                    var percent = Math.Round((double) (100 * user.Value) / guildMessages, 2);
                    highScore.AppendLine(
                        $"{NumerToEmoji(highscorePlace)} **{user.Key.Username}#{user.Key.Discriminator}** - {percent}% of total - {user.Value} messages");
                    highscorePlace++;
                }
                await ReplyAsync(highScore.ToString());
            }
            catch (Exception e)
            {
                errorHandler.HandleCommandException(e, Context);
            }
        }

        private string NumerToEmoji(int number)
        {
            var emojis = new string[] {":one:", ":two:", ":three:", ":four:", ":five:", ":six", ":seven:", ":eight:", ":nine:", ":keycap_ten:"};
            try
            {
                return emojis[number - 1];
            }
            catch (Exception e)
            {
                logger.Warning(e, $"Can't provide emoji number {number}");
                return ":zero:";
            }
        }
    }
}