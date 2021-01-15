using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Friendplant.Data;
using System.Text.RegularExpressions;
using Friendplant.Data.ProfileElements;

namespace Friendplant.Commands {
    public class CMGeneric {

        [Command("help")]
        public async Task CHelp(CommandContext ctx, string command = null) {

            switch(command) {

                case null:
                    DiscordEmbedBuilder embed;
                    embed = new DiscordEmbedBuilder {
                        Title = "Справочник по командам",
                        Description = $"**Создатель**: {ctx.Client.GetUserAsync(354297822691983371).Result.Mention}\n**Язык программирования**: C#\n**Библиотека**: [DSharpPlus](https://github.com/DSharpPlus)\n**Код бота**: [Github](https://github.com/Avatcher/Friendplant)\n*v.{Vars.Version}*",
                        Color = Vars.ColorBlue,
                        ThumbnailUrl = ctx.Client.CurrentUser.AvatarUrl
                    };

                    embed.AddField("Также...", "Для дополнительной ифнормации о команде введите `*help <Команда>`.", false);

                    string spisok = "```\n*profile\n*profile.color\n*profile.notice\n```";
                    embed.AddField("Профиль", spisok, true);

                    spisok = "```\n*transfer\n*casino\n*shop\n*buy\n```";
                    embed.AddField("Блестяшки", spisok, true);

                    spisok = "```\n*rnd\n*rndfrom\n```";
                    embed.AddField("Рандом", spisok, true);

                    await ctx.Channel.SendMessageAsync(embed: embed);
                    break;

                case "profile":
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = "Профиль",
                        Description = $"`*profile` - Показывает ваш профиль.\n`*profile <User>` - Показывает профиль пользователя.",
                        Color = Vars.ColorBlue
                    });
                    break;

                case "profile.color":
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = "Профиль",
                        Description = $"`*profile.color <HEX>` - Меняет цвет оконтовки вашего профиля. Цвет должен быть в формате HEX.",
                        Color = Vars.ColorBlue
                    });
                    break;

                case "profile.notice":
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = "Профиль",
                        Description = $"`*profile.notice <ON/OFF>` - Включает/Выключает сообщения Бота о новом уровне.",
                        Color = Vars.ColorBlue
                    });
                    break;

                case "transfer":
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = "Блестяшки",
                        Description = $"`*transfer <User> <Amount>` - Переводит блестяшки на счет другого пользователя.",
                        Color = Vars.ColorBlue
                    });
                    break;

                case "casino":
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = "Блестяшки",
                        Description = $"`*casino` - Стоит `2`блестяшки, Джекпот `20`блестяшек.",
                        Color = Vars.ColorBlue
                    });
                    break;

                case "shop":
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = "Блестяшки",
                        Description = $"`*shop` - Выводит магазин со списком его товаров.",
                        Color = Vars.ColorBlue
                    });
                    break;

                case "buy":
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = "Блестяшки",
                        Description = $"`*buy <Number>` - Покупает предмет из магазина под номером Number.",
                        Color = Vars.ColorBlue
                    });
                    break;

                case "rnd":
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = "Рандом",
                        Description = $"`*rnd <num1> <num2>` - Выводит случайное число между num1 и num2.",
                        Color = Vars.ColorBlue
                    });
                    break;

                case "rndfrom":
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = "Рандом",
                        Description = $"`*rndfrom <word1> <word2> ...` - Выдает случайное слово из списка, \"используйте кавычки для нескольких слов\"",
                        Color = Vars.ColorBlue
                    });
                    break;

                default:
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = $":bell: Неизвестная команда \"{command}\"",
                        Color = Vars.ColorRed
                    });
                    break;
            }
            
        } 

        [Command("profile")]
        public async Task CProfile(CommandContext ctx, DiscordUser user = null) {
            if(user == null) user = ctx.User;
            if(user.IsBot) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":robot: Бот не может иметь профиль.",
                    Color = new DiscordColor(0x55acee)
                });
                return;
            }

            new Profile(user);

            Profile pro = Vars.Humanity[user.Id];

            string MoneyRank;
            int MoneyIndex = Array.IndexOf(Sorting.GetHumanityTop(Sorting.By.Money), pro);
            if(MoneyIndex == 0) MoneyRank = Vars.Emoji["medal1"];
            else if(MoneyIndex == 1) MoneyRank = Vars.Emoji["medal2"];
            else if(MoneyIndex == 2) MoneyRank = Vars.Emoji["medal3"];
            else MoneyRank = Vars.Emoji["medal4"] + Convert.ToString(MoneyIndex+1);

            string ExpRank;
            int ExpIndex   = Array.IndexOf(Sorting.GetHumanityTop(Sorting.By.Exp), pro);
            if(ExpIndex == 0) ExpRank = Vars.Emoji["medal1"];
            else if(ExpIndex == 1) ExpRank = Vars.Emoji["medal2"];
            else if(ExpIndex == 2) ExpRank = Vars.Emoji["medal3"];
            else ExpRank = Vars.Emoji["medal4"] + Convert.ToString(ExpIndex + 1);

            string sparkle = pro.Radioactive ? Vars.Emoji["pluto"] : Vars.Emoji["sparkle"];
            string exp = pro.Radioactive ? Vars.Emoji["gexp"] : Vars.Emoji["bexp"];

            var embed = new DiscordEmbedBuilder {
                Author = new DiscordEmbedBuilder.EmbedAuthor{
                    Name = $"{user.Username}#{user.Discriminator}",
                    IconUrl = user.AvatarUrl
                },
                Title = pro.Radioactive ? "Радиоактивный профиль" : "Профиль",
                Color = new DiscordColor(pro.Color),
                Footer = new DiscordEmbedBuilder.EmbedFooter {
                    Text = "Id: "+user.Id
                }
            };

            embed.AddField($"{MoneyRank} Баланс: `{pro.Balance.Money}`{sparkle}", $"Потрачено: `{pro.Balance.Spent}`{sparkle}", true);

            embed.AddField($"{ExpRank} Уровень: `{pro.Level.Amount}`", $"Опыт: `{pro.Level.Experience}`/`{pro.Level.ExpNeeding}`{exp}", true);

            var his = pro.Balance.History;
            string historyString = "";
            if(pro.Balance.History.Count != 0) {
                foreach(HistoryElement h in his) {
                    historyString += $"{h.Date} ▸ `{h.Amount}`{sparkle} ▸ {h.Description}\n";
                }
                historyString.Remove(historyString.Length - 1);
            }
            else {
                historyString = "Пусто...";
            }

            embed.AddField($"{Vars.Emoji["paper"]}История платежей", historyString);

            await ctx.Channel.SendMessageAsync(embed:embed);
        }
        
        [Command("profile.color")]
        public async Task CProfileColor(CommandContext ctx, string code) {
            new Profile(ctx.User);

            Regex reg = new Regex(@"^#{0,1}[a-fA-F0-9]{6}$");
            if(!reg.Match(code).Success) {
                await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                    Title = ":art: Введите HEX код цвета.",
                    Color = new DiscordColor(0xdd2e44)
                });
                return;
            }

            int color = Convert.ToInt32(code, 16);

            Vars.Humanity[ctx.User.Id].Color = color;

            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Title = ":art: Цвет профиля изменен на #"+code,
                Color = new DiscordColor(color)
            });
        }
    
        [Command("profile.notice")]
        public async Task CProfileNotifications(CommandContext ctx, string what) {
            new Profile(ctx.User);

            switch(what.ToLower()) {

                case "true":
                case "enable":
                case "1":
                case "on":
                    Vars.Humanity[ctx.User.Id].Level.Muted = false;
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder { 
                        Title = ":bell: Уведомления о новом уровне ВКЛЮЧЕНЫ",
                        Color = Vars.ColorBlue
                    });

                    break;

                case "false":
                case "disable":
                case "0":
                case "off":
                    Vars.Humanity[ctx.User.Id].Level.Muted = true;
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = ":bell: Уведомления о новом уровне ВЫКЛЮЧЕНЫ",
                        Color = Vars.ColorBlue
                    });

                    break;

                default:
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = $":bell: Неизвестное состояние \"{what}\"",
                        Color = Vars.ColorRed
                    });

                    break;
            }
        }

        [Command("top")]
        public async Task CTop(CommandContext ctx, string mode = null) {

            new Profile(ctx.User);

            Profile[] profilesArr = new Profile[Vars.Humanity.Count];
            Vars.Humanity.Values.CopyTo(profilesArr, 0);
            int userIndex = 999; string top = "";
            DiscordEmbedBuilder embed;
            DiscordUser usr;

            switch(mode.ToLower()) {

                case "level":

                    profilesArr = Sorting.GetHumanityTop(Sorting.By.Exp);
                    userIndex = Array.IndexOf(profilesArr, Vars.Humanity[ctx.User.Id]);

                    for(int i = 0; i < profilesArr.Length; i++) {

                        usr = ctx.Client.GetUserAsync(profilesArr[i].Id).GetAwaiter().GetResult();

                        if(i == 0)      top += $"{Vars.Emoji["medal1"]} - `{profilesArr[i].Level.Amount}`УР - {usr.Mention}\n";
                        else if(i == 1) top += $"{Vars.Emoji["medal2"]} - `{profilesArr[i].Level.Amount}`УР - {usr.Mention}\n";
                        else if(i == 2) top += $"{Vars.Emoji["medal3"]} - `{profilesArr[i].Level.Amount}`УР - {usr.Mention}\n";
                        else            top += $"**{i+1}** - `{profilesArr[i].Level.Amount}`УР - {usr.Mention} \n";
                    }

                    if(userIndex == 0)      top += $"\nВаше место: {Vars.Emoji["medal1"]}**{userIndex+1}** - `{profilesArr[userIndex].Level.Amount}`УР - {ctx.User.Mention}";
                    else if(userIndex == 1) top += $"\nВаше место: {Vars.Emoji["medal2"]}**{userIndex+1}** - `{profilesArr[userIndex].Level.Amount}`УР - {ctx.User.Mention}";
                    else if(userIndex == 2) top += $"\nВаше место: {Vars.Emoji["medal3"]}**{userIndex+1}** - `{profilesArr[userIndex].Level.Amount}`УР - {ctx.User.Mention}";
                    else                    top += $"\nВаше место: {Vars.Emoji["medal4"]}**{userIndex+1}** - `{profilesArr[userIndex].Level.Amount}`УР - {ctx.User.Mention}";

                    embed = new DiscordEmbedBuilder {
                        Title = "Таблица лидеров по уровню",
                        Description = top,
                        Color = Vars.ColorBlue
                    };

                    await ctx.Channel.SendMessageAsync(embed:embed);

                    break;

                case "money":

                    profilesArr = Sorting.GetHumanityTop(Sorting.By.Money);
                    userIndex = Array.IndexOf(profilesArr, Vars.Humanity[ctx.User.Id]);

                    for(int i = 0; i < profilesArr.Length; i++) {
                        usr = ctx.Client.GetUserAsync(profilesArr[i].Id).Result;

                        if(i == 0)      top += $"{Vars.Emoji["medal1"]} - `{profilesArr[i].Balance.Money}`{Vars.Emoji["sparkle"]} - {usr.Mention}\n";
                        else if(i == 1) top += $"{Vars.Emoji["medal2"]} - `{profilesArr[i].Balance.Money}`{Vars.Emoji["sparkle"]} - {usr.Mention}\n";
                        else if(i == 2) top += $"{Vars.Emoji["medal3"]} - `{profilesArr[i].Balance.Money}`{Vars.Emoji["sparkle"]} - {usr.Mention}\n";
                        else            top += $"**{i+1}** - `{profilesArr[i].Balance.Money}`{Vars.Emoji["sparkle"]} - {usr.Mention}\n";
                    }
                    if     (userIndex == 0) top += $"\nВаше место: {Vars.Emoji["medal1"]} - `{profilesArr[userIndex].Balance.Money}`{Vars.Emoji["sparkle"]} - {ctx.User.Mention}";
                    else if(userIndex == 1) top += $"\nВаше место: {Vars.Emoji["medal2"]} - `{profilesArr[userIndex].Balance.Money}`{Vars.Emoji["sparkle"]} - {ctx.User.Mention}";
                    else if(userIndex == 2) top += $"\nВаше место: {Vars.Emoji["medal3"]} - `{profilesArr[userIndex].Balance.Money}`{Vars.Emoji["sparkle"]} - {ctx.User.Mention}";
                    else                    top += $"\nВаше место: {Vars.Emoji["medal4"]}**{userIndex+1}** - `{profilesArr[userIndex].Balance.Money}`{Vars.Emoji["sparkle"]} - {ctx.User.Mention}";

                    embed = new DiscordEmbedBuilder {
                        Title = "Таблица лидеров по блестяшкам",
                        Description = top,
                        Color = Vars.ColorBlue
                    };

                    await ctx.Channel.SendMessageAsync(embed: embed);

                    break;

                default:
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Title = $":no_entry: Неизвестный рейтинг \"{mode}\",\nУ нас есть только `Level` и `Money`",
                        Color = Vars.ColorRed
                    });
                    return;
            }
            
        }
    
        [Command("emojis")]
        public async Task CEMojis(CommandContext ctx) {
            string res = "";
            foreach(string emoji in Vars.Emoji.Values) {
                res += emoji;
            }
            await ctx.Channel.SendMessageAsync(res);
        }
    
        [Command("rnd")]
        public async Task CRnd(CommandContext ctx, int num1, int num2) {
            await ctx.Channel.SendMessageAsync($"Взято `{new Random().Next(num1, num2+1)}` из {num1} по {num2}");
        }

        [Command("rndfrom")]
        public async Task CRndFrom(CommandContext ctx, params string[] choises) {
            await ctx.Channel.SendMessageAsync($"Взято `{choises[new Random().Next(0, choises.Length)]}`");
        }

    }
}
