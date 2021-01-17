using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Friendplant.Data;
using Friendplant.Data.ProfileElements;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;
using DSharpPlus.Interactivity;
using System.IO;
using Newtonsoft.Json;

namespace Friendplant.Commands {
    public class CMMoney {
        
        [Command("transfer")]
        public async Task CPay(CommandContext ctx, DiscordUser user, int count) {

            if(user.IsBot) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":robot: Бот не может иметь баланс.",
                    Color = new DiscordColor(0x55acee)
                });
                return;
            }
            if(user == ctx.User) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":no_entry: Это так не работает! Введите другого пользователя для перевода.",
                    Color = Vars.ColorRed
                });
                return;
            }

            new Profile(ctx.User); new Profile(user);

            if(count < 0) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":no_entry: Число для перевода не может быть отрицательным.",
                    Color = Vars.ColorRed
                });
                return;
            }
            if(count > Vars.Humanity[ctx.User.Id].Balance.Money && !Vars.Humanity[ctx.User.Id].Radioactive) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":moneybag: На вашем балансе недостаточно блестяшек.",
                    Color = Vars.ColorRed
                });
                return;
            }

            var inter = ctx.Client.GetInteractivityModule();


            var msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Title = "Подтвержение...",
                Description = $"Вы уверены, что хотите перевести `{count}`{Vars.Emoji["sparkle"]}блестяшек {user.Mention}?\n:white_check_mark: - для подтверждения.",
                Color = Vars.ColorBlue
            });
            var emo = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var emo2 = DiscordEmoji.FromName(ctx.Client, ":no_entry:");

            await msg.CreateReactionAsync(emo);
            await msg.CreateReactionAsync(emo2);

            if(inter.WaitForMessageReactionAsync(msg, ctx.User).Result.Emoji == emo) {
                Vars.Humanity[ctx.User.Id].Balance.Transfer(count * -1, true, $"-{count}", "Перевод на " + user.Mention);
                Vars.Humanity[user.Id].Balance.Transfer(count, false, $"+{count}", "Перевод от " + ctx.User.Mention);

                await msg.DeleteAllReactionsAsync();
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder {
                    Title = $":moneybag: Перевод `{count}`{Vars.Emoji["sparkle"]} блестяшек прошел успешно.",
                    Color = Vars.ColorBlue
                });
            }
            else {
                await msg.DeleteAllReactionsAsync();
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder {
                    Title = $":moneybag: ПЕРЕВОД ОТМЕНЕН",
                    Color = Vars.ColorBlue
                });
            }
        }

        [Command("casino")]
        public async Task CCasino(CommandContext ctx) {
            new Profile(ctx.User);

            if(Vars.Humanity[ctx.User.Id].Balance.Money < 2 && !Vars.Humanity[ctx.User.Id].Radioactive) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":moneybag: На вашем балансе недостаточно блестяшек.",
                    Color = Vars.ColorRed
                });
                return;
            }


            string[] emojis = { ":cold_face:", ":nauseated_face:", ":smiling_imp:", ":clown:", ":alien:" };
            string[] slots = {
                emojis[new Random().Next(0, emojis.Length)],
                emojis[new Random().Next(0, emojis.Length)],
                emojis[new Random().Next(0, emojis.Length)],
            };

            string result = "|";
            foreach(string element in slots) {
                result += element + "|";
            }

            var color = new DiscordColor(0xdd2e44); bool win = false;
            if(slots[0] == slots[1] && slots[1] == slots[2]) {
                switch(slots[0]) {

                    case ":cold_face:":
                        color = new DiscordColor(0x50a5e6);
                        break;

                    case ":nauseated_face:":
                        color = new DiscordColor(0x77b255);
                        break;

                    case ":smiling_imp:":
                        color = new DiscordColor(0xaa8dd8);
                        break;

                    case ":clown:":
                        color = new DiscordColor(0xfee7b8);
                        break;

                    case ":alien:":
                        color = new DiscordColor(0xccd6dd);
                        break;
                }
                win = true;
                Vars.Humanity[ctx.User.Id].Balance.Transfer(30, false, "+30", "Джекпот в казино");
            }
            else Vars.Humanity[ctx.User.Id].Balance.Transfer(-2, true, "-2", "Проигрыш в казино");

            var embed = new DiscordEmbedBuilder {
                Title = result,
                Description = win ? $":tada: {ctx.User.Mention}, вам повезло и вы выиграли `30`{Vars.Emoji["sparkle"]} блестяшек!" : $":red_circle: {ctx.User.Mention}, вы проигрываете `2`{Vars.Emoji["sparkle"]} блестяшки.",
                Color = color
            };

            await ctx.Channel.SendMessageAsync(embed: embed);
        }
    
        [Command("shop")]
        public async Task CShop(CommandContext ctx) {
            var embed = new DiscordEmbedBuilder {
                Title = ":shopping_cart: Магазин",
                Description = "Что-бы что-нибудь купить, введите `*buy <Номер товара>`.",
                Color = Vars.ColorBlue
            };

            int i = 0;
            foreach(RoleItem item in Vars.Shop.Values) {
                i++;
                embed.AddField($"**{i}** - `{item.Name}` - `{item.Cost}`{Vars.Emoji["sparkle"]}", item.Desc);
            }

            await ctx.Channel.SendMessageAsync(embed: embed);
        }

        [Command("buy")]
        public async Task CBuy(CommandContext ctx, int num) {

            new Profile(ctx.User);

            // Numbers of items
            int i = 0; Dictionary<int, ulong> dict = new Dictionary<int, ulong>();
            foreach(RoleItem item in Vars.Shop.Values) {
                i++;
                dict[i] = item.Id;
            }

            // List of member roles
            List<ulong> list = new List<ulong>();
            foreach(DiscordRole role in ctx.Member.Roles) {
                list.Add(role.Id);
            }

            DiscordMessage msg; DiscordEmoji emo; DiscordEmoji emo2;

            // No item with choosen number
            if(num <= 0 || num > i) {
                Console.WriteLine("bruh");
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = $":shopping_cart: В магазине нету товара под номером `{num}`",
                    Color = Vars.ColorRed
                });
                return;
            }

            // Member already have item
            if(Vars.Humanity[ctx.User.Id].Items.Contains(dict[num]) && list.Contains(Vars.Shop[dict[num]].Id)) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = $":shopping_cart: У вас уже есть этот предмет.",
                    Color = Vars.ColorRed
                });
                return;
            }

            // Item is in Member's profile, but member haven't role
            if(Vars.Humanity[ctx.User.Id].Items.Contains(dict[num]) && !list.Contains(Vars.Shop[dict[num]].Id)) {
                msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":shopping_cart: Подтверждение...",
                    Description = $"Кажется, вы уже покупали `{Vars.Shop[dict[num]].Name}` ранее, хотите востановить его?\n:white_check_mark: - для подтверждения.",
                    Color = Vars.ColorBlue
                });

                emo = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                emo2 = DiscordEmoji.FromName(ctx.Client, ":no_entry:");

                await msg.CreateReactionAsync(emo);
                await msg.CreateReactionAsync(emo2);

                // Restore Confirm
                if(ctx.Client.GetInteractivityModule().WaitForMessageReactionAsync(msg, ctx.User).Result.Emoji == emo) {
                    await msg.DeleteAllReactionsAsync();

                    if(list.Contains(751149673934225408))
                        await ctx.Member.RevokeRoleAsync(ctx.Guild.GetRole(751149673934225408));
                    
                    await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(dict[num]));
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder {
                        Title = ":shopping_cart: Предмет успешно восстановлен.",
                        Description = "",
                        Color = Vars.ColorBlue
                    });
                    return;
                }
                // Cancel
                else {
                    await msg.DeleteAllReactionsAsync();
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder {
                        Title = ":shopping_cart: ВОССТАНОВЛЕНИЕ ОТМЕНЕНО.",
                        Description = "",
                        Color = Vars.ColorRed
                    });
                    return;
                }
            }

            // Not enough sparkles
            if(Vars.Humanity[ctx.User.Id].Balance.Money < Vars.Shop[dict[num]].Cost && !Vars.Humanity[ctx.User.Id].Radioactive) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = $":shopping_cart: У вас недостаточно блестяшек, чтобы купить этот предмет.",
                    Color = Vars.ColorRed
                });
                return;
            }

            //// Buying item

            msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Title = ":shopping_cart: Подтвержение...",
                Description = $"Вы уверены, что хотите приобрести `{Vars.Shop[dict[num]].Name}` за `{Vars.Shop[dict[num]].Cost}`{Vars.Emoji["sparkle"]}блестяшек?\n:white_check_mark: - для подтверждения.",
                Color = Vars.ColorBlue
            });
            emo = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            emo2 = DiscordEmoji.FromName(ctx.Client, ":no_entry:");

            await msg.CreateReactionAsync(emo);
            await msg.CreateReactionAsync(emo2);

            // Buying confirm
            if(ctx.Client.GetInteractivityModule().WaitForMessageReactionAsync(msg, ctx.User).Result.Emoji == emo) {
                Vars.Humanity[ctx.User.Id].Balance.Transfer(
                    Vars.Shop[dict[num]].Cost * -1, true, $"{Vars.Shop[dict[num]].Cost * -1}", $"Покупка `{Vars.Shop[dict[num]].Name}`");
                Vars.Humanity[ctx.User.Id].Items.Add(dict[num]);

                await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(dict[num]));

                await msg.DeleteAllReactionsAsync();
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder {
                    Title = ":shopping_cart: Покупка прошла успешно!",
                    Description = "",
                    Color = Vars.ColorBlue
                });
            }
            // Cancel
            else {
                await msg.DeleteAllReactionsAsync();
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder {
                    Title = ":shopping_cart: ПОКУПКА ОТМЕНЕНА.",
                    Description = "",
                    Color = Vars.ColorBlue
                });
            }
        }
    
        [Command("newcode")]
        public async Task CNewCode(CommandContext ctx, int cost){
            new Profile(ctx.User);

            if(cost > Vars.Humanity[ctx.User.Id].Balance.Money && !Vars.Humanity[ctx.User.Id].Radioactive){
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = $":convenience_store: У вас недостаточно блестяшек, чтобы создать этот подарочный код.",
                    Color = Vars.ColorRed
                });
            }
            if(cost <= 0) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = $":convenience_store: Ты... Это так не работает...",
                    Color = Vars.ColorRed
                });
            }

            var cde = new PrizeCode(ctx.User, cost);

            var dm = await ctx.Client.CreateDmAsync(ctx.User);
            await dm.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Title = $"Код: ||{cde.Code}||",
                Description = $"`*usecode <CODE>` - Чтобы использовать код.",
                Color = Vars.ColorBlue
            });
            await dm.DeleteAsync();

            if(ctx.Channel.Type != DSharpPlus.ChannelType.Private) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":label: Подарочный код был отправлен в личные сообщения.",
                    Color = Vars.ColorBlue
                });
            }

            File.WriteAllText(Vars.CodesPath, JsonConvert.SerializeObject(Vars.Codes));
        }
    
        [Command("usecode")]
        public async Task CUseCode(CommandContext ctx, string code){
            new Profile(ctx.User);

            code = code.ToUpper();

            List<string> codesList = new List<string>();
            foreach(string key in Vars.Codes.Keys) {
                codesList.Add(key);
            }

            if(!codesList.Contains(code)){
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = $":label: Кажется, код \"**{code}**\" либо уже использован, либо никогда не существовал.",
                    Color = Vars.ColorRed
                });
                return;
            }

            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Title = $":label: Вы получили `{Vars.Codes[code].Cost}`{Vars.Emoji["sparkle"]}блестяшек!",
                Color = Vars.ColorBlue
            });

            Vars.Codes[code].Use(ctx.User);
            File.WriteAllText(Vars.CodesPath, JsonConvert.SerializeObject(Vars.Codes));
        }
    }
}
