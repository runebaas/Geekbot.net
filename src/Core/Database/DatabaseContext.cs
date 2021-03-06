﻿using Geekbot.Core.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Geekbot.Core.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<QuoteModel> Quotes { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<GuildSettingsModel> GuildSettings { get; set; }
        public DbSet<KarmaModel> Karma { get; set; }
        public DbSet<ShipsModel> Ships { get; set; }
        public DbSet<RollsModel> Rolls { get; set; }
        public DbSet<MessageSeasonsModel> MessagesSeasons { get; set; }
        public DbSet<MessagesModel> Messages { get; set; }
        public DbSet<SlapsModel> Slaps { get; set; }
        public DbSet<GlobalsModel> Globals { get; set; }
        public DbSet<RoleSelfServiceModel> RoleSelfService { get; set; }
        public DbSet<CookiesModel> Cookies { get; set; }
        public DbSet<ReactionListenerModel> ReactionListeners { get; set; }
    }
}