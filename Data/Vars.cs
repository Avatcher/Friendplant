using System;
using System.Collections.Generic;
using System.Text;

namespace Friendplant.Data {
    public class Vars {

        public static string HumanityPath = @"..\..\..\Data\Files\Humanity.bin";
        public static Dictionary<ulong, Profile> Humanity = new Dictionary<ulong, Profile>();

        public static Dictionary<string, string> Emoji = new Dictionary<string, string>();
    }
}
