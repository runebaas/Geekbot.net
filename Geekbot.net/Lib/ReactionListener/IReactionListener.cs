﻿using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Geekbot.net.Lib.ReactionListener
{
    public interface IReactionListener
    {
        bool IsListener(ulong id);
        Task AddRoleToListener(string messageId, IEmote emoji, IRole role);
        void RemoveRole(ISocketMessageChannel channel, SocketReaction reaction);
        void GiveRole(ISocketMessageChannel message, SocketReaction reaction);
    }
}