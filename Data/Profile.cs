using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using Friendplant.Data.ProfileElements;
using Newtonsoft.Json;

namespace Friendplant.Data {
    /// <summary>
    /// Профиль
    /// </summary>
    [Serializable]
    public class Profile {
        public ulong Id { get; private set; } // Айди пользователя профиля
        public Balance Balance { get; set; }  // Баланс пользователя
        public Level Level { get; set; }      // Уровень пользователя
        public int Color { get; set; }        // Цвет профиля
        public bool Radioactive { get; set; } // Радиоактивность (Админка)

        public List<ulong> Items = new List<ulong>(); // Купленные предметы

        public Profile(DiscordUser user) {
            // Запрет ботам иметь профиль
            if(user.IsBot) throw new ArgumentException("Bot can't have got own profile.");

            // Пользователь уже имеет баланс, отменяем
            if(Vars.Humanity.ContainsKey(user.Id)) return;

            Id = user.Id;
            Balance = new Balance();
            Level = new Level();
            Color = 0x7289da;
            Radioactive = false;

            // Сохраняем в Базу Данных
            Vars.Humanity[user.Id] = this;

            // Оповещение
            Console.WriteLine($"< {user.Username}#{user.Discriminator} created new Profile({user.Id})");
        }
    
        [JsonConstructor]
        public Profile(ulong id, Balance bal, Level lev, int col, bool rad){
            Id = id; Balance = bal; Level = lev; Color = col; Radioactive = rad;
        }
    }
}
