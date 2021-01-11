using System;
using System.Collections.Generic;
using System.Text;

namespace Friendplant.Data {
    public class Vars {

        public const string Version = "1.0.8";

        public const string Token = "...";

        public const string Prefix = "*";
        public const ulong AvatcherId = 354297822691983371;

        public static string HumanityPath = @"..\..\..\Data\Files\Humanity.bin";
        public static Dictionary<ulong, Profile> Humanity = new Dictionary<ulong, Profile>();

        public static Dictionary<string, string> Emoji = new Dictionary<string, string>();

    }
}
