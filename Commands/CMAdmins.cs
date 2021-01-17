using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Friendplant.Data;
using Friendplant.Data.ProfileElements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Friendplant.Commands {
    public class CMAdmins {

        [Command("*item"), RequirePermissions(Permissions.Administrator)]
        public async Task CItem(CommandContext ctx, DiscordRole role, int cost, string description) {
            new RoleItem(role, cost, description);
            if(Vars.Shop.ContainsKey(role.Id)){
                await ctx.Channel.SendMessageAsync("Предмет успешно отредактирован!");
            }
            else{
                await ctx.Channel.SendMessageAsync("Новый предмет успешно создан!");
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            File.WriteAllText(Vars.ShopPath, JsonConvert.SerializeObject(Vars.Shop));
        }
        
        [Command("*deleteitem"), RequirePermissions(Permissions.Administrator)]
        public async Task CDeleteItem(CommandContext ctx, DiscordRole role){
            var msg = await ctx.Channel.SendMessageAsync("Вы уверены, что хотите удалить этот предмет?");

            var emo = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var emo2 = DiscordEmoji.FromName(ctx.Client, ":no_entry:");

            await msg.CreateReactionAsync(emo);
            await msg.CreateReactionAsync(emo2);

            if(ctx.Client.GetInteractivityModule().WaitForMessageReactionAsync(msg, ctx.User).Result.Emoji == emo){
                Vars.Shop.Remove(role.Id);
                File.WriteAllText(Vars.ShopPath, JsonConvert.SerializeObject(Vars.Shop));
                await msg.DeleteAllReactionsAsync();
                await msg.ModifyAsync("Предмет успешно удален.");
            }
            else{
                await msg.DeleteAllReactionsAsync();
                await msg.ModifyAsync("Удаление отменено.");
            }
        }
    
    }
}
