using DSharpPlus.Entities;
using Friendplant.Data.ProfileElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Friendplant.Data {
    public class PrizeCode {
        public ulong OwnerId { get; private set; }
        public string Code { get; set; }
        public int Cost { get; set; }

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

            Vars.Humanity[user.Id].Balance.Transfer(
                cost*-1, new HistoryElement(
                    Vars.Humanity[user.Id].Balance,
                    $"-{cost}", "Создан подарочный код"
                ), true
            );

            Console.WriteLine($"< New code({Code}) created.");

            // Saving code
            Vars.Codes[Code] = this;
        }

        public void Use(DiscordUser user) {

            Vars.Humanity[user.Id].Balance.Transfer(
                Cost, new HistoryElement(
                    Vars.Humanity[user.Id].Balance,
                    $"+{Cost}",
                    "Использован подарочный код"
                ), false
                );

            // Delete code
            Vars.Codes.Remove(Code);
        }
    }
}
