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
                    Color = new DiscordColor(0xbe1931)
                });
                return;
            }

            new Profile(ctx.User); new Profile(user);

            if(count < 0) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":no_entry: Число для перевода не может быть отрицательным.",
                    Color = new DiscordColor(0xbe1931)
                });
                return;
            }
            if(count > Vars.Humanity[ctx.User.Id].Balance.Money && !Vars.Humanity[ctx.User.Id].Radioactive) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":moneybag: На вашем балансе недостаточно блестяшек.",
                    Color = new DiscordColor(0xbe1931)
                });
                return;
            }

            Vars.Humanity[ctx.User.Id].Balance.Transfer(count*-1, new HistoryElement(Vars.Humanity[ctx.User.Id].Balance, "-" + count, "Перевод на " + user.Mention), true);
            Vars.Humanity[user.Id].Balance.Transfer(count, new HistoryElement(Vars.Humanity[user.Id].Balance, "+" + count, "Перевод от " + ctx.User.Mention), false);

            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Title = $":moneybag: Перевод `{count}`{Vars.Emoji["sparkle"]} блестяшек на {user.Mention}.",
                Color = DiscordColor.Green
            });
        }

        [Command("casino")]
        public async Task CCasino(CommandContext ctx) {
            new Profile(ctx.User);

            if(Vars.Humanity[ctx.User.Id].Balance.Money < 3 && !Vars.Humanity[ctx.User.Id].Radioactive) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":moneybag: На вашем балансе недостаточно блестяшек.",
                    Color = new DiscordColor(0xdd2e44)
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
                Vars.Humanity[ctx.User.Id].Balance.Transfer(30, new HistoryElement(Vars.Humanity[ctx.User.Id].Balance, "+30", "Джекпот в казино"), false);
            }
            else Vars.Humanity[ctx.User.Id].Balance.Transfer(-2, new HistoryElement(Vars.Humanity[ctx.User.Id].Balance, "-2", "Проигрыш в казино"), true);

            var embed = new DiscordEmbedBuilder {
                Title = result,
                Description = win ? $":tada: {ctx.User.Mention}, вам повезло и вы выиграли `30`{Vars.Emoji["sparkle"]} блестяшек!" : $":red_circle: {ctx.User.Mention}, вы проигрываете `2`{Vars.Emoji["sparkle"]} блестяшки.",
                Color = color
            };

            await ctx.Channel.SendMessageAsync(embed: embed);
        }
    
    }
}
