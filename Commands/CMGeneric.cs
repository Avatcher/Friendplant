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
        public async Task CHelp(CommandContext ctx) {

            DiscordEmbedBuilder embed;
            embed = new DiscordEmbedBuilder {
                Title = "Справочник по командам",
                Description = $"**Создатель**: {ctx.Client.GetUserAsync(354297822691983371).Result.Mention}\n**Язык программирования**: C#\n**Библиотека**: [DSharpPlus](https://github.com/DSharpPlus)\n*v.0.0.3*",
                Color = ctx.Guild.CurrentMember.Color
            };

            string spisok = @$"`*profile` - Показывает ваш профиль
`*profile <User/Id>` - Показывает профиль пользователя
`*profile.color <HEX color>` - Меняет цвет вашего профиля
`*transfer <User/Id> <Amount>` - Переводит {Vars.Emoji["sparkle"]}блестяшки на счет пользователя
`*casino` - Казино. Стоит 2{Vars.Emoji["sparkle"]}, джекпот 30{Vars.Emoji["sparkle"]}";
            embed.AddField("Капитал", spisok, true);     
                    
            await ctx.Channel.SendMessageAsync(embed: embed);
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

            string sparkle = pro.Radioactive ? Vars.Emoji["pluto"] : Vars.Emoji["sparkle"];

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

            embed.AddField($"Баланс: `{pro.Balance.Money}`{sparkle}", $"Потрачено: `{pro.Balance.Spent}`{sparkle}", true);

            embed.AddField($"Уровень: `{pro.Level.Amount}`", $"Опыт: `{pro.Level.Experience}`/`{pro.Level.ExpNeeding}`", true);

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
    }
}
