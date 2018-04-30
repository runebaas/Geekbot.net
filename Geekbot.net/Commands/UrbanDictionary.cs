﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Geekbot.net.Lib;
using Newtonsoft.Json;

namespace Geekbot.net.Commands
{
    public class UrbanDictionary : ModuleBase
    {
        private readonly IErrorHandler _errorHandler;

        public UrbanDictionary(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        [Command("urban", RunMode = RunMode.Async)]
        [Remarks(CommandCategories.Helpers)]
        [Summary("Lookup something on urban dictionary")]
        public async Task UrbanDefine([Remainder] [Summary("word")] string word)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://api.urbandictionary.com");
                    var response = await client.GetAsync($"/v0/define?term={word}");
                    response.EnsureSuccessStatusCode();

                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var definitions = JsonConvert.DeserializeObject<UrbanResponse>(stringResponse);
                    if (definitions.List.Count == 0)
                    {
                        await ReplyAsync("That word hasn't been defined...");
                        return;
                    }

                    var definition = definitions.List.First(e => !string.IsNullOrWhiteSpace(e.Example));

                    var eb = new EmbedBuilder();
                    eb.WithAuthor(new EmbedAuthorBuilder
                    {
                        Name = definition.Word,
                        Url = definition.Permalink
                    });
                    eb.WithColor(new Color(239, 255, 0));
                    if (!string.IsNullOrEmpty(definition.Definition)) eb.Description = definition.Definition;
                    if (!string.IsNullOrEmpty(definition.Example)) eb.AddField("Example", definition.Example ?? "(no example given...)");
                    if (!string.IsNullOrEmpty(definition.ThumbsUp)) eb.AddInlineField("Upvotes", definition.ThumbsUp);
                    if (!string.IsNullOrEmpty(definition.ThumbsDown)) eb.AddInlineField("Downvotes", definition.ThumbsDown);
                    if (definitions.Tags.Length > 0) eb.AddField("Tags", string.Join(", ", definitions.Tags));

                    await ReplyAsync("", false, eb.Build());
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleCommandException(e, Context);
            }
        }

        private class UrbanResponse
        {
            public string[] Tags { get; set; }
            public List<UrbanListItem> List { get; set; }
        }

        private class UrbanListItem
        {
            public string Definition { get; set; }
            public string Permalink { get; set; }
            public string ThumbsUp { get; set; }
            public string Word { get; set; }
            public string Example { get; set; }
            public string ThumbsDown { get; set; }
        }
    }
}