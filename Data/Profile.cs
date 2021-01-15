using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using Friendplant.Data.ProfileElements;

namespace Friendplant.Data {
    [Serializable]
    public class Profile {
        public ulong Id { get; private set; }
        public Balance Balance { get; set; }
        public Level Level { get; set; }
        public int Color { get; set; }
        public bool Radioactive { get; set; }
        public List<ulong> Items = new List<ulong>();

        public Profile(DiscordUser user) {
            if(user.IsBot) throw new ArgumentException("Bot can't have got own profile.");
            if(Vars.Humanity.ContainsKey(user.Id)) return;

            Id = user.Id;

            Balance = new Balance();
            Level = new Level();

            Color = 0x7289da;
            Radioactive = false;

            // Save to DataBase
            Vars.Humanity[user.Id] = this;

            Console.WriteLine($"< {user.Username}#{user.Discriminator} created new Profile({user.Id})");
        }
    }
}
