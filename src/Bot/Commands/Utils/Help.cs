﻿using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Geekbot.Core.ErrorHandling;

namespace Geekbot.Bot.Commands.Utils
{
    public class Help : ModuleBase
    {
        private readonly IErrorHandler _errorHandler;

        public Help(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("List all Commands")]
        public async Task GetHelp()
        {
            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("For a list of all commands, please visit the following page");
                sb.AppendLine("https://geekbot.pizzaandcoffee.rocks/commands");
                var dm = await Context.User.GetOrCreateDMChannelAsync();
                await dm.SendMessageAsync(sb.ToString());
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context);
            }
        }
    }
}