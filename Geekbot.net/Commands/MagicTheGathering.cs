﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Geekbot.net.Lib;
using MtgApiManager.Lib.Service;
using Serilog;

namespace Geekbot.net.Commands
{
    public class Magicthegathering : ModuleBase
    {
        private ILogger _logger;
        private IErrorHandler _errorHandler;

        public Magicthegathering(ILogger logger, IErrorHandler errorHandler)
        {
            _logger = logger;
            _errorHandler = errorHandler;
        }

        [Command("mtg", RunMode = RunMode.Async)]
        [Remarks(CommandCategories.Games)]
        [Summary("Find a Magic The Gathering Card.")]
        public async Task getCard([Remainder] [Summary("name")] string cardName)
        {
            try
            {
                var service = new CardService();
                var result = service.Where(x => x.Name, cardName);

                var card = result.All().Value.FirstOrDefault();
                if (card == null)
                {
                    await ReplyAsync("I couldn't find that card...");
                    return;
                }
                var eb = new EmbedBuilder();
                eb.Title = card.Name;
                eb.Description = card.Type;
                
                if (card.Colors != null) eb.WithColor(GetColor(card.Colors));

                if (card.ImageUrl != null) eb.ImageUrl = card.ImageUrl.ToString();

                if (!string.IsNullOrEmpty(card.Text)) eb.AddField("Text", card.Text);
                
                if (!string.IsNullOrEmpty(card.Flavor)) eb.AddField("Flavor", card.Flavor);
                if (!string.IsNullOrEmpty(card.SetName)) eb.AddInlineField("Set", card.SetName);
                if (!string.IsNullOrEmpty(card.Power)) eb.AddInlineField("Power", card.Power);
                if (!string.IsNullOrEmpty(card.Loyalty)) eb.AddInlineField("Loyality", card.Loyalty);
                if (!string.IsNullOrEmpty(card.Toughness)) eb.AddInlineField("Thoughness", card.Toughness);
                
                if (!string.IsNullOrEmpty(card.ManaCost)) eb.AddInlineField("Cost", card.ManaCost);
                if (!string.IsNullOrEmpty(card.Rarity)) eb.AddInlineField("Rarity", card.Rarity);
                
                if (card.Legalities != null) eb.AddField("Legality", string.Join(", ", card.Legalities.Select(e => e.Format)));

                await ReplyAsync("", false, eb.Build());
            }
            catch (Exception e)
            {
                _errorHandler.HandleCommandException(e, Context);
            }
        }

        private Color GetColor(IEnumerable<string> colors)
        {
            var color = colors.FirstOrDefault();
            switch (color)
            {
                case "Black":
                    return new Color(177, 171, 170);
                case "White":
                    return new Color(255, 252, 214);
                case "Blue":
                    return new Color(156, 189, 204);
                case "Red":
                    return new Color(204, 156, 140);
                case "Green":
                    return new Color(147, 181, 159);
                default:
                    return new Color(255, 252, 214);
            }
        }
    }
}