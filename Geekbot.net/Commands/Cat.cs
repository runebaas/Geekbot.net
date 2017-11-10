﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Geekbot.net.Lib;
using Newtonsoft.Json;

namespace Geekbot.net.Commands
{
    public class Cat : ModuleBase
    {
        private readonly IErrorHandler _errorHandler;
        
        public Cat(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }
        
        [Command("cat", RunMode = RunMode.Async)]
        [Remarks(CommandCategories.Randomness)]
        [Summary("Return a random image of a cat.")]
        public async Task Say()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri("http://random.cat");
                        var response = await client.GetAsync("/meow.php");
                        response.EnsureSuccessStatusCode();

                        var stringResponse = await response.Content.ReadAsStringAsync();
                        var catFile = JsonConvert.DeserializeObject<CatResponse>(stringResponse);
                        await ReplyAsync(catFile.file);
                    }
                    catch (HttpRequestException e)
                    {
                        await ReplyAsync($"Seems like the dog cought the cat (error occured)\r\n{e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleCommandException(e, Context);
            }
        }
    }

    public class CatResponse
    {
        public string file { get; set; }
    }
}