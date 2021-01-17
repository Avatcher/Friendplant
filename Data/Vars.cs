using DSharpPlus.Entities;
using Friendplant.Data.ProfileElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Friendplant.Data {
    public class Vars {

        public static string Version = "1.1.1";

        public static string Token = File.ReadAllText(@"D:\token.txt");

        public static string Prefix = "*";
        public static ulong AvatcherId = 354297822691983371;

        public static DiscordColor ColorRed = new DiscordColor(0xbe1931);
        public static DiscordColor ColorBlue = new DiscordColor(0x7289da);

        public static Dictionary<string, string> Emoji = new Dictionary<string, string>();

        public static string HumanityPath = @"..\..\..\Data\Files\Humanity.json";
        public static Dictionary<ulong, Profile> Humanity = new Dictionary<ulong, Profile>();

        public static string ShopPath = @"..\..\..\Data\Files\Shop.json";
        public static Dictionary<ulong, RoleItem> Shop = new Dictionary<ulong, RoleItem>();

        public static string CodesPath = @"..\..\..\Data\Files\Codes.json";
        public static Dictionary<string, PrizeCode> Codes = new Dictionary<string, PrizeCode>();
    }
}
