﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Serilog;
using StackExchange.Redis;
using Utf8Json;

namespace Geekbot.net.Lib
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabase _redis;
        private readonly ILogger _logger;
        public UserRepository(IDatabase redis, ILogger logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public Task<bool> Update(SocketUser user)
        {
            try
            {
                var savedUser = Get(user.Id);
                savedUser.Id = user.Id;
                savedUser.Username = user.Username;
                savedUser.Discriminator = user.Discriminator;
                savedUser.AvatarUrl = user.GetAvatarUrl() ?? "0";
                savedUser.IsBot = user.IsBot;
                savedUser.Joined = user.CreatedAt;
                if(savedUser.UsedNames == null) savedUser.UsedNames = new List<string>();
                if (!savedUser.UsedNames.Contains(user.Username))
                {
                    savedUser.UsedNames.Add(user.Username);
                }
                Store(savedUser);
                
                _logger.Information($"[UserRepository] Updated User {user.Username}#{user.Discriminator} ({user.Id})");
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _logger.Warning(e, $"[UserRepository] Failed to update {user.Username}#{user.Discriminator} ({user.Id})");
                return Task.FromResult(false);
            }
        }

        private void Store(UserRepositoryUser user)
        {
            _redis.HashSetAsync($"Users:{user.Id.ToString()}", new HashEntry[]
            {
                new HashEntry("Id", user.Id.ToString()),
                new HashEntry("Username", user.Username),
                new HashEntry("Discriminator", user.Discriminator),
                new HashEntry("AvatarUrl", user.AvatarUrl),
                new HashEntry("IsBot", user.IsBot), 
                new HashEntry("Joined", user.Joined.ToString()), 
                new HashEntry("UsedNames", JsonSerializer.Serialize(user.UsedNames)), 
            });
        }

        public UserRepositoryUser Get(ulong userId)
        {
            try
            {
                var user = _redis.HashGetAll($"Users:{userId.ToString()}");
                for (int i = 1; i < 11; i++)
                {
                    if (user.Length != 0) break;
                    user = _redis.HashGetAll($"Users:{(userId + (ulong) i).ToString()}");

                }
                var dto = new UserRepositoryUser();
                foreach (var a in user.ToDictionary())
                {
                    switch (a.Key)
                    {
                        case "Id":
                            dto.Id = ulong.Parse(a.Value);
                            break;
                        case "Username":
                            dto.Username = a.Value.ToString();
                            break;
                        case "Discriminator":
                            dto.Discriminator = a.Value.ToString();
                            break;
                        case "AvatarUrl":
                            dto.AvatarUrl = (a.Value != "0") ? a.Value.ToString() : null;
                            break;
                        case "IsBot":
                            dto.IsBot = a.Value == 1;
                            break;
                        case "Joined":
                            dto.Joined = DateTimeOffset.Parse(a.Value.ToString());
                            break;
                        case "UsedNames":
                            dto.UsedNames = JsonSerializer.Deserialize<List<string>>(a.Value.ToString()) ?? new List<string>();
                            break;
                    }
                }
                return dto;
            }
            catch (Exception e)
            {
                _logger.Warning(e, $"[UserRepository] Failed to get {userId} from repository");
                return new UserRepositoryUser();
            }
        }

        public string getUserSetting(ulong userId, string setting)
        {
            return _redis.HashGet($"Users:{userId}", setting);
        }

        public bool saveUserSetting(ulong userId, string setting, string value)
        {
            _redis.HashSet($"Users:{userId}", new HashEntry[]
            {
                new HashEntry(setting, value)
            });
            return true;
        }
    }

    public class UserRepositoryUser
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsBot { get; set; }
        public DateTimeOffset Joined { get; set; }
        public List<string> UsedNames { get; set; }
    }

    public interface IUserRepository
    {
        Task<bool> Update(SocketUser user);
        UserRepositoryUser Get(ulong userId);
        string getUserSetting(ulong userId, string setting);
        bool saveUserSetting(ulong userId, string setting, string value);
    }
}