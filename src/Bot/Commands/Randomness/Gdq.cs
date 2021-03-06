﻿using System;
using System.Net;
using System.Threading.Tasks;
using Discord.Commands;
using Geekbot.Core.ErrorHandling;

namespace Geekbot.Bot.Commands.Randomness
{
    public class Gdq : ModuleBase
    {
        private readonly IErrorHandler _errorHandler;
        
        public Gdq(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        [Command("gdq", RunMode = RunMode.Async)]
        [Summary("Get a quote from the GDQ donation generator.")]
        public async Task GetQuote()
        {
            try
            {
                using var client = new WebClient();
                var url = new Uri("http://taskinoz.com/gdq/api/");
                var response = client.DownloadString(url);
                    
                await ReplyAsync(response);
            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context);
            }
        }
    }
}