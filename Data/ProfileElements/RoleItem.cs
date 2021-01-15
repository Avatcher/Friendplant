using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Friendplant.Data.ProfileElements {

    [Serializable]
    public class RoleItem {
        public ulong Id { get; set; }
        public int Cost { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        public RoleItem(DiscordRole role, int cost, string description) {
            Id = role.Id;
            Cost = cost;
            Name = role.Name;
            Desc = description;

            Vars.Shop[Id] = this;
        }
    }
}
