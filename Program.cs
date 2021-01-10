using Friendplant.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System.IO;

namespace Friendplant {
    class Program {
        static void Main(string[] args) {
            Console.Title = "Friendplant console";

            // Bot creating
            var bot = new Bot();

            // Start Bot
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
