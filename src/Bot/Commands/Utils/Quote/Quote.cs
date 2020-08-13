using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Geekbot.Core.CommandPreconditions;
using Geekbot.Core.Database;
using Geekbot.Core.Database.Models;
using Geekbot.Core.ErrorHandling;
using Geekbot.Core.Extensions;
using Geekbot.Core.Localization;
using Geekbot.Core.Polyfills;
using Geekbot.Core.RandomNumberGenerator;

namespace Geekbot.Bot.Commands.Utils.Quote
{
    [Group("quote")]
    [DisableInDirectMessage]
    public class Quote : ModuleBase
    {
        private readonly IErrorHandler _errorHandler;
        private readonly DatabaseContext _database;
        private readonly IRandomNumberGenerator _randomNumberGenerator;
        private readonly ITranslationHandler _translationHandler;

        public Quote(IErrorHandler errorHandler, DatabaseContext database, IRandomNumberGenerator randomNumberGenerator, ITranslationHandler translationHandler)
        {
            _errorHandler = errorHandler;
            _database = database;
            _randomNumberGenerator = randomNumberGenerator;
            _translationHandler = translationHandler;
        }

        [Command]
        [Summary("Return a random quoute from the database")]
        public async Task GetRandomQuote()
        {
            try
            {
                var s = _database.Quotes.Where(e => e.GuildId.Equals(Context.Guild.Id.AsLong())).ToList();
                
                if (!s.Any())
                {
                    var transContext = await _translationHandler.GetGuildContext(Context);
                    await ReplyAsync(transContext.GetString("NoQuotesFound"));
                    return;
                }

                var random = _randomNumberGenerator.Next(0, s.Count);
                var quote = s[random];
                
                var embed = QuoteBuilder(quote);
                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context, "Whoops, seems like the quote was to edgy to return");
            }
        }

        [Command("add")]
        [Alias("save")]
        [Summary("Add a quote from the last sent message by @user")]
        public async Task AddQuote([Summary("@someone")] IUser user)
        {
            try
            {
                var transContext = await _translationHandler.GetGuildContext(Context);
                if (user.Id == Context.Message.Author.Id)
                {
                    await ReplyAsync(transContext.GetString("CannotSaveOwnQuotes"));
                    return;
                }

                if (user.IsBot)
                {
                    await ReplyAsync(transContext.GetString("CannotQuoteBots"));
                    return;
                }

                var lastMessage = await GetLastMessageByUser(user);
                if (lastMessage == null) return;
                
                var quote = CreateQuoteObject(lastMessage);
                await _database.Quotes.AddAsync(quote);
                await _database.SaveChangesAsync();

                var embed = QuoteBuilder(quote);
                await ReplyAsync(transContext.GetString("QuoteAdded"), false, embed.Build());
            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context,
                    "I counldn't find a quote from that user :disappointed:");
            }
        }

        [Command("add")]
        [Alias("save")]
        [Summary("Add a quote from a message id")]
        public async Task AddQuote([Summary("message-ID")] ulong messageId)
        {
            try
            {
                var transContext = await _translationHandler.GetGuildContext(Context);
                var message = await Context.Channel.GetMessageAsync(messageId);
                if (message.Author.Id == Context.Message.Author.Id)
                {
                    await ReplyAsync(transContext.GetString("CannotSaveOwnQuotes"));
                    return;
                }

                if (message.Author.IsBot)
                {
                    await ReplyAsync(transContext.GetString("CannotQuoteBots"));
                    return;
                }

                var quote = CreateQuoteObject(message);
                await _database.Quotes.AddAsync(quote);
                await _database.SaveChangesAsync();
                
                var embed = QuoteBuilder(quote);
                await ReplyAsync(transContext.GetString("QuoteAdded"), false, embed.Build());
            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context,
                    "I couldn't find a message with that id :disappointed:");
            }
        }
        
        [Command("add")]
        [Alias("save")]
        [Summary("Add a quote from a message link")]
        public async Task AddQuote([Summary("message-link")] string messageLink)
        {
            try
            {
                var transContext = await _translationHandler.GetGuildContext(Context);
                
                if (!MessageLink.IsValid(messageLink))
                {
                    await ReplyAsync(transContext.GetString("NotAValidMessageLink"));
                    return;
                }
                
                var link = new MessageLink(messageLink);
                if (link.GuildId != Context.Guild.Id)
                {
                    await ReplyAsync(transContext.GetString("OnlyQuoteFromSameServer"));
                    return;
                }
                
                var channel = link.ChannelId == Context.Channel.Id
                    ? Context.Channel
                    : await Context.Guild.GetTextChannelAsync(link.ChannelId);

                var message = await channel.GetMessageAsync(link.MessageId);
                
                if (message.Author.Id == Context.Message.Author.Id)
                {
                    await ReplyAsync(transContext.GetString("CannotSaveOwnQuotes"));
                    return;
                }
                
                if (message.Author.IsBot)
                {
                    await ReplyAsync(transContext.GetString("CannotQuoteBots"));
                    return;
                }

                var quote = CreateQuoteObject(message);
                await _database.Quotes.AddAsync(quote);
                await _database.SaveChangesAsync();
                
                var embed = QuoteBuilder(quote);
                await ReplyAsync(transContext.GetString("QuoteAdded"), false, embed.Build());
            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context,
                    "I couldn't find a message with that id :disappointed:");
            }
        }

        [Command("make")]
        [Summary("Create a quote from the last sent message by @user")]
        public async Task ReturnSpecifiedQuote([Summary("@someone")] IUser user)
        {
            try
            {
                var lastMessage = await GetLastMessageByUser(user);
                if (lastMessage == null) return;
                var quote = CreateQuoteObject(lastMessage);
                var embed = QuoteBuilder(quote);
                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context,
                    "I counldn't find a quote from that user :disappointed:");
            }
        }

        [Command("make")]
        [Summary("Create a quote from a message id")]
        public async Task ReturnSpecifiedQuote([Summary("message-ID")] ulong messageId)
        {
            try
            {
                var message = await Context.Channel.GetMessageAsync(messageId);
                var quote = CreateQuoteObject(message);
                var embed = QuoteBuilder(quote);
                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context,
                    "I couldn't find a message with that id :disappointed:");
            }
        }
        
        [Command("make")]
        [Summary("Create a quote from a message link")]
        public async Task ReturnSpecifiedQuote([Summary("message-link")] string messageLink)
        {
            try
            {
                var transContext = await _translationHandler.GetGuildContext(Context);
                
                if (!MessageLink.IsValid(messageLink))
                {
                    await ReplyAsync(transContext.GetString("NotAValidMessageLink"));
                    return;
                }
                
                var link = new MessageLink(messageLink);
                if (link.GuildId != Context.Guild.Id)
                {
                    await ReplyAsync(transContext.GetString("OnlyQuoteFromSameServer"));
                    return;
                }

                var channel = link.ChannelId == Context.Channel.Id
                    ? Context.Channel
                    : await Context.Guild.GetTextChannelAsync(link.ChannelId);

                var message = await channel.GetMessageAsync(link.MessageId);
                var quote = CreateQuoteObject(message);
                var embed = QuoteBuilder(quote);
                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context,
                    "I couldn't find that message :disappointed:");
            }
        }
        
       [Command("remove")]
       [RequireUserPermission(GuildPermission.ManageMessages)]
       [Summary("Remove a quote (user needs the 'ManageMessages' permission)")]
       public async Task RemoveQuote([Summary("quote-ID")] int id)
       {
           try
           {
               var transContext = await _translationHandler.GetGuildContext(Context);
               var quote = _database.Quotes.Where(e => e.GuildId == Context.Guild.Id.AsLong() && e.InternalId == id)?.FirstOrDefault();
               if (quote != null)
               {
                   _database.Quotes.Remove(quote);
                   await _database.SaveChangesAsync();
                   var embed = QuoteBuilder(quote);
                   await ReplyAsync(transContext.GetString("Removed", id), false, embed.Build());
               }
               else
               {
                   await ReplyAsync(transContext.GetString("NotFoundWithId"));
               }
           }
           catch (Exception e)
           {
               await _errorHandler.HandleCommandException(e, Context, "I couldn't find a quote with that id :disappointed:");
           }
       }

       [Command("stats")]
       [Summary("Show quote stats for this server")]
       public async Task GetQuoteStatsForServer()
       {
           try
           {
               // setup
               var transContext = await _translationHandler.GetGuildContext(Context);
               var eb = new EmbedBuilder();
               eb.Author = new EmbedAuthorBuilder()
               {
                   IconUrl = Context.Guild.IconUrl,
                   Name = $"{Context.Guild.Name} - {transContext.GetString("QuoteStats")}"
               };

               // gather data
               var totalQuotes = _database.Quotes.Count(row => row.GuildId == Context.Guild.Id.AsLong());
               if (totalQuotes == 0)
               {
                   // no quotes, no stats, end of the road
                   await ReplyAsync(transContext.GetString("NoQuotesFound"));
                   return;
               }
               
               var mostQuotedPerson = _database.Quotes
                   .Where(row => row.GuildId == Context.Guild.Id.AsLong())
                   .GroupBy(row => row.UserId)
                   .Select(row => new { userId = row.Key, amount = row.Count()})
                   .OrderBy(row => row.amount)
                   .Last();
               var mostQuotedPersonUser = Context.Client.GetUserAsync(mostQuotedPerson.userId.AsUlong()).Result ?? new UserPolyfillDto {Username = "Unknown User"};

               var quotesByYear = _database.Quotes
                   .Where(row => row.GuildId == Context.Guild.Id.AsLong())
                   .GroupBy(row => row.Time.Year)
                   .Select(row => new { year = row.Key, amount = row.Count()})
                   .OrderBy(row => row.year);
               
               // add data to the embed
               eb.AddField(transContext.GetString("MostQuotesPerson"), $"{mostQuotedPersonUser.Username} ({mostQuotedPerson.amount})");
               eb.AddInlineField(transContext.GetString("TotalQuotes"), totalQuotes);

               foreach (var year in quotesByYear)
               {
                   eb.AddInlineField(year.year.ToString(), year.amount);
               }

               await ReplyAsync("", false, eb.Build());
           }
           catch (Exception e)
           {
               await _errorHandler.HandleCommandException(e, Context);
           }
       }

        private async Task<IMessage> GetLastMessageByUser(IUser user)
        {
            try
            {
                var list = Context.Channel.GetMessagesAsync().Flatten();
                return await list.FirstOrDefaultAsync(msg => 
                    msg.Author.Id == user.Id &&
                    msg.Embeds.Count == 0 &&
                    msg.Id != Context.Message.Id &&
                    !msg.Content.ToLower().StartsWith("!"));
            }
            catch
            {
                await ReplyAsync($"No quoteable message have been sent by {user.Username} in this channel");
                return null;
            }
        }

        private EmbedBuilder QuoteBuilder(QuoteModel quote)
        {
            var user = Context.Client.GetUserAsync(quote.UserId.AsUlong()).Result ?? new UserPolyfillDto { Username = "Unknown User" };
            var eb = new EmbedBuilder();
            eb.WithColor(new Color(143, 167, 232));
            if (quote.InternalId == 0)
            {
                eb.Title = $"{user.Username} @ {quote.Time.Day}.{quote.Time.Month}.{quote.Time.Year}";                
            }
            else
            {
                eb.Title = $"#{quote.InternalId} | {user.Username} @ {quote.Time.Day}.{quote.Time.Month}.{quote.Time.Year}";                
            }
            eb.Description = quote.Quote;
            eb.ThumbnailUrl = user.GetAvatarUrl();
            if (quote.Image != null) eb.ImageUrl = quote.Image;
            return eb;
        }

        private QuoteModel CreateQuoteObject(IMessage message)
        {
            string image;
            try
            {
                image = message.Attachments.First().Url;
            }
            catch (Exception)
            {
                image = null;
            }

            var last = _database.Quotes.Where(e => e.GuildId.Equals(Context.Guild.Id.AsLong())).OrderByDescending(e => e.InternalId).FirstOrDefault();
            var internalId = 0;
            if (last != null) internalId = last.InternalId + 1;
            return new QuoteModel()
            {
                InternalId = internalId,
                GuildId = Context.Guild.Id.AsLong(),
                UserId = message.Author.Id.AsLong(),
                Time = message.Timestamp.DateTime,
                Quote = message.Content,
                Image = image
            };
        }
    }
}