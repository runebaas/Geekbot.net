﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Geekbot.net.Database;
using Geekbot.net.Database.Models;
using Geekbot.net.Lib.Extensions;
using Geekbot.net.Lib.Logger;

namespace Geekbot.net.Lib.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _database;
        private readonly IGeekbotLogger _logger;
        public UserRepository(DatabaseContext database, IGeekbotLogger logger)
        {
            _database = database;
            _logger = logger;
        }

        public async Task<bool> Update(SocketUser user)
        {
            try
            {
                var savedUser = Get(user.Id);
                var isNew = false;
                if (savedUser == null)
                {
                    savedUser = new UserModel();
                    isNew = true;
                }
                savedUser.UserId = user.Id.AsLong();
                savedUser.Username = user.Username;
                savedUser.Discriminator = user.Discriminator;
                savedUser.AvatarUrl = user.GetAvatarUrl() ?? "";
                savedUser.IsBot = user.IsBot;
                savedUser.Joined = user.CreatedAt;
                if (savedUser.UsedNames == null)  savedUser.UsedNames = Enumerable.Empty<string>().ToArray();
                if (!savedUser.UsedNames.Contains(user.Username))
                {
                    savedUser.UsedNames = savedUser.UsedNames.Concat(new[] {user.Username}).ToArray();
                }

                if (isNew)
                {
                    await _database.Users.AddAsync(savedUser);
                }
                else
                {
                    _database.Users.Update(savedUser);
                }

                await _database.SaveChangesAsync();

                _logger.Information(LogSource.UserRepository, "Updated User", savedUser);
                await Task.Delay(500);
                return true;
            }
            catch (Exception e)
            {
                _logger.Warning(LogSource.UserRepository, $"Failed to update user: {user.Username}#{user.Discriminator} ({user.Id})", e);
                return false;
            }
        }

        public UserModel Get(ulong userId)
        {
            try
            {
                return _database.Users.FirstOrDefault(u => u.UserId.Equals(userId.AsLong()));
            }
            catch (Exception e)
            {
                _logger.Warning(LogSource.UserRepository, $"Failed to get {userId} from repository", e);
                return null;
            }
        }
    }
}