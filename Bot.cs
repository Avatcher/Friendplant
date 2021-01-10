using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Friendplant.Data;
using Friendplant.Data.ProfileElements;
using System.IO;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace Friendplant {
    public class TopSecret {

        public const string Token = "Token";
        public const string Prefix = "*";
        public const ulong AvatcherId = 354297822691983371;
    }
    class Bot {
        public DiscordClient Client { get; private set; }
        public CommandsNextModule CommandsModule { get; private set; }

        private bool DoAutosave;
        private int AutosaveEveryHeartbeat = 3;
        private int AutosaveIn = 0;

        public async Task RunAsync() {

            // Creating Configuration
            var config = new DiscordConfiguration {
                Token = TopSecret.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
            };

            // Creating Client
            Client = new DiscordClient(config);

            // Bot events
            Client.Ready += OnClientReady;
            Client.Heartbeated += OnHeartbeat;
            Client.GuildAvailable += OnGuildAvailible;
            Client.MessageCreated += OnMessageCreated;

            // Creating Commands Configuration
            var commandsConfig = new CommandsNextConfiguration {
                StringPrefix = TopSecret.Prefix,
                IgnoreExtraArguments = false,
                EnableMentionPrefix = true,   // Allow to use @Friendplant as prefix
                EnableDefaultHelp = false     // Disable default help command
            };

            Client.UseInteractivity(new InteractivityConfiguration {
                PaginationBehaviour = TimeoutBehaviour.Default,
                Timeout = TimeSpan.FromSeconds(30)
            });

            // Creating Commands Module
            CommandsModule = Client.UseCommandsNext(commandsConfig);

            // Registring Commands Modules
            CommandsModule.RegisterCommands<Commands.CMGeneric>(); // Generic and Standart Commands
            CommandsModule.RegisterCommands<Commands.CMMoney>();   // Money Commands
            //////////////////////////////

            // Final Operations
            await Client.ConnectAsync(); // Enable Bot

            // Command console + infinity loop
            while(true) {
                string[] answer = Console.ReadLine().Split(' ');
                switch(answer[0]) {

                    case "close":
                    case "c":
                        BinSer.Serialize(Vars.Humanity, File.Open(Vars.HumanityPath, FileMode.Open));
                        Console.WriteLine(">> Humanity.bin was updated.");
                        Console.WriteLine(">> Closing program...");

                        Client.DisconnectAsync().GetAwaiter().GetResult();
                        Environment.Exit(0); // Close program
                        break;

                    case "load":
                        Vars.Humanity = BinSer.Deserialize<Dictionary<ulong, Profile>>(File.Open(Vars.HumanityPath, FileMode.Open));
                        Console.WriteLine(">> Humanity was loaded from the file.");
                        break;

                    case "save":
                    case "s":
                        BinSer.Serialize(Vars.Humanity, File.Open(Vars.HumanityPath, FileMode.Open));
                        Console.WriteLine(">> Humanity.bin was updated.");
                        break;

                    case "list":
                        Console.WriteLine($">>> Humanity contains {Vars.Humanity.Count} profiles");
                        Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.Black;
                        foreach(ulong b in Vars.Humanity.Keys) {
                            DiscordUser bUser = Client.GetUserAsync(b).Result;
                            Console.WriteLine($"> {bUser.Username}#{bUser.Discriminator} - {b}");
                        }
                        Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                        break;

                    case "radiate":
                        ulong rid = (ulong)Convert.ToInt64(answer[1]);
                        DiscordUser usr = Client.GetUserAsync(rid).Result;

                        Vars.Humanity[rid].Radioactive = !Vars.Humanity[rid].Radioactive;
                        Console.WriteLine($">> {usr.Username}#{usr.Discriminator} get a dose of radiation.");
                        break;

                    case "clear":
                        Vars.Humanity.Clear();
                        Console.WriteLine(">> Humanity was cleared.");
                        break;

                    case "autosave":
                        DoAutosave = !DoAutosave;
                        Console.WriteLine(">> Autosave is now " + DoAutosave.ToString());
                        break;

                    default:
                        Console.WriteLine($">> Unknown command \"{answer[0]}\"");
                        break;
                }
            }       
        }

        private Task OnClientReady(ReadyEventArgs e) {

            // Set Activity
            Client.UpdateStatusAsync(
                game: new DiscordGame {
                    Name = "Переделывается..."
                },
                user_status: UserStatus.DoNotDisturb
                );

            // Do Autosave
            DoAutosave = true;

            // Bank restoring
            try {
                Vars.Humanity = BinSer.Deserialize<Dictionary<ulong, Profile>>(
                    File.Exists(Vars.HumanityPath) ?
                    File.Open(Vars.HumanityPath, FileMode.Open) :
                    File.Open(Vars.HumanityPath, FileMode.Create));

                Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($">>> Profiles succesfully restored with {Vars.Humanity.Count} amount."); Console.ResetColor();

                Console.BackgroundColor = ConsoleColor.Magenta; Console.ForegroundColor = ConsoleColor.Black;
                foreach(ulong key in Vars.Humanity.Keys) {

                    DiscordUser bUser = Client.GetUserAsync(key).Result;
                    Console.WriteLine($"> {bUser.Username}#{bUser.Discriminator} - {key}");
                }
                Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
            }
            catch(System.Runtime.Serialization.SerializationException) {
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($">>> Profiles.bin file is badly formed."); Console.ResetColor();
            }

            return Task.CompletedTask;
        }

        private Task OnHeartbeat(HeartbeatEventArgs e) {

            if(!DoAutosave) return Task.CompletedTask;

            if(AutosaveIn < AutosaveEveryHeartbeat) {
                AutosaveIn++;
                return Task.CompletedTask;
            }
            else {
                AutosaveIn = 0;
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(">>> Profiles.bin autosaving...");
            BinSer.Serialize(Vars.Humanity, File.Open(Vars.HumanityPath, FileMode.Open));
            Console.WriteLine(">>> Profiles.bin succesfully saved.");
            Console.ResetColor();

            return Task.CompletedTask;
        }

        private Task OnMessageCreated(MessageCreateEventArgs e) {
            if(e.Author.IsBot) return Task.CompletedTask; // Bot ignore

            new Profile(e.Author);

            if(Vars.Humanity[e.Author.Id].Level.AddExp()) { // Return true, if levelUp

                int blesk = Convert.ToInt32(5 * 0.5 * Vars.Humanity[e.Author.Id].Level.Amount); // Prize sparkles
                Vars.Humanity[e.Author.Id].Balance.Transfer(blesk, new HistoryElement(Vars.Humanity[e.Author.Id].Balance, "+"+blesk, "Новый уровень"), false);

                e.Channel.SendMessageAsync($"> :tada: Поздравляю, {e.Author.Mention}, ты достиг `{Vars.Humanity[e.Author.Id].Level.Amount}` уровня!\n> Ты получаешь `{blesk}`{Vars.Emoji["sparkle"]} блестяшек!");
            }

            return Task.CompletedTask;
        }

        private Task OnGuildAvailible(GuildCreateEventArgs e) {

            if(e.Guild.Id == 797103909424332811) { // Adding emoji from Friendplant Case server
                foreach(DiscordEmoji emoji in e.Guild.Emojis) {
                    Vars.Emoji[emoji.Name] = $"<:{emoji.Name}:{emoji.Id}>";
                }
            }

            return Task.CompletedTask;
        }
    }
}
