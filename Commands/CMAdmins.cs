using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friendplant.Data;
using Friendplant.Data.ProfileElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Friendplant.Commands {
    public class CMAdmins {

        [Command("*newitem"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task CNewItem(CommandContext ctx, DiscordRole role, int cost, string description) {
            new RoleItem(role, cost, description);
            await ctx.Channel.SendMessageAsync("Новый предмет успешно создан!");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(">>> Shop.bin autosaving...");
            BinSer.Serialize(Vars.Shop, File.Open(Vars.ShopPath, FileMode.Open));
            Console.WriteLine(">>> Shop.bin succesfully saved.");
        }
    }
}
