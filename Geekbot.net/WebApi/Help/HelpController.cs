﻿using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Geekbot.net.Lib;
using Nancy;

namespace Geekbot.net.WebApi.Help
{
    public class HelpController : NancyModule
    {
        public HelpController()
        {
            Get("/v1/commands", args =>
            {
                var commands = GetCommands().Result;

                var commandList = (from cmd in commands.Commands
                    let cmdParamsObj = cmd.Parameters.Select(cmdParam => new CommandParamDto
                        {
                            Summary = cmdParam.Summary,
                            Default = cmdParam.DefaultValue?.ToString() ?? null,
                            Type = cmdParam.Type?.ToString()
                        })
                        .ToList()
                    let param = string.Join(", !", cmd.Aliases)
                    select new CommandDto
                    {
                        Name = cmd.Name,
                        Summary = cmd.Summary,
                        Category = cmd.Remarks ?? CommandCategories.Uncategorized,
                        IsAdminCommand = (param.Contains("admin")),
                        Aliases = cmd.Aliases.ToArray(),
                        Params = cmdParamsObj
                    }).ToList();
                return Response.AsJson(commandList);
                
            });
        }

        private async Task<CommandService> GetCommands()
        {
            var commands = new CommandService();
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
            return commands;
        }
    }
}