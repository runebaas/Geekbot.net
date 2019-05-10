using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Geekbot.net.Database;
using Geekbot.net.Database.Models;
using Geekbot.net.Lib.CommandPreconditions;
using Geekbot.net.Lib.ErrorHandling;
using Geekbot.net.Lib.Extensions;
using Geekbot.net.Lib.Localization;

namespace Geekbot.net.Commands.Rpg
{
    [DisableInDirectMessage]
    [Group("cookie")]
    public class Cookies : ModuleBase
    {
        private readonly DatabaseContext _database;
        private readonly IErrorHandler _errorHandler;
        private readonly ITranslationHandler _translation;

        public Cookies(DatabaseContext database, IErrorHandler errorHandler, ITranslationHandler translation)
        {
            _database = database;
            _errorHandler = errorHandler;
            _translation = translation;
        }

        [Command("get", RunMode = RunMode.Async)]
        [Summary("Get a cookie every 24 hours")]
        public async Task GetCookies()
        {
            try
            {
                var actor = await GetUser(Context.User.Id);
                if (actor.LastPayout.Value.AddHours(24) > DateTimeOffset.Now)
                {
                    await ReplyAsync($"You already got cookies in the last 24 hours, wait until {actor.LastPayout.Value.AddHours(24):HH:mm:ss} for more cookies");
                    return;
                }
                actor.Cookies += 10;
                actor.LastPayout = DateTimeOffset.Now;
                await SetUser(actor);
                await ReplyAsync($"You got 10 cookies, there are now {actor.Cookies} cookies in you cookie jar");

            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context);
            }
        }
        
        [Command("jar", RunMode = RunMode.Async)]
        [Summary("Look at your cookie jar")]
        public async Task PeekIntoCookieJar()
        {
            try
            {
                var actor = await GetUser(Context.User.Id);
                await ReplyAsync($"There are {actor.Cookies} cookies in you cookie jar");

            }
            catch (Exception e)
            {
                await _errorHandler.HandleCommandException(e, Context);
            }
        }
        
        private async Task<CookiesModel> GetUser(ulong userId)
        {
            var user = _database.Cookies.FirstOrDefault(u =>u.GuildId.Equals(Context.Guild.Id.AsLong()) && u.UserId.Equals(userId.AsLong())) ?? await CreateNewRow(userId);
            return user;
        }
        
        private async Task SetUser(CookiesModel user)
        {
            _database.Cookies.Update(user);
            await _database.SaveChangesAsync();
        }
        
        private async Task<CookiesModel> CreateNewRow(ulong userId)
        {
            var user = new CookiesModel()
            {
                GuildId = Context.Guild.Id.AsLong(),
                UserId = userId.AsLong(),
                Cookies = 0,
                LastPayout = DateTimeOffset.MinValue
            };
            var newUser = _database.Cookies.Add(user).Entity;
            await _database.SaveChangesAsync();
            return newUser;
        }
    }
}
