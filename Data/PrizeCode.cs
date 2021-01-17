using DSharpPlus.Entities;
using Friendplant.Data.ProfileElements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Friendplant.Data {
    /// <summary>
    /// Подарочный код.
    /// </summary>
    public class PrizeCode {
        public ulong OwnerId { get; private set; } // Айдишник создателя кода
        public string Code { get; set; }           // Сам код
        public int Cost { get; set; }              // Деньги получаемые за код

        public PrizeCode(DiscordUser user, int cost) {

            if(cost > Vars.Humanity[user.Id].Balance.Money && !Vars.Humanity[user.Id].Radioactive) {
                throw new ArgumentException("Not enough money.");
            }

            OwnerId = user.Id; Cost = cost;

            List<string> codesList = new List<string>();
            foreach(string key in Vars.Codes.Keys) {
                codesList.Add(key);
            }

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; string code = "";
            while(true) {
                for(int i = 0 ; i < 4 ; i++) {
                    code += chars[new Random().Next(0, chars.Length)];
                }
                if(!codesList.Contains(code)){
                    break;
                }
            }
            Code = code;

            Vars.Humanity[user.Id].Balance.Transfer(cost*-1, true, $"-{cost}", "Создан подарочный код");

            Console.WriteLine($"< New code({Code}) created.");

            // Saving code
            Vars.Codes[Code] = this;
        }

        [JsonConstructor]
        public PrizeCode(ulong id, string code, int cost) {
            OwnerId = id; Code = code; Cost = cost;
        }

        /// <summary>
        /// Использует подарочный код
        /// </summary>
        /// <param name="user">Пользователь, использующий код.</param>
        public void Use(DiscordUser user) {

            Vars.Humanity[user.Id].Balance.Transfer(Cost, false, $"+{Cost}", "Использован подарочный код");

            // Delete code
            Vars.Codes.Remove(Code);
        }
    }
}
