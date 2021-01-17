﻿using DSharpPlus;
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
using Newtonsoft.Json;

namespace Friendplant {

    class Bot {
        public DiscordClient Client { get; private set; }
        public InteractivityModule Interactivity { get; private set; }
        public CommandsNextModule CommandsModule { get; private set; }

        private bool DoAutosave;
        private int AutosaveEveryHeartbeat = 5;
        private int AutosaveIn = 0;

        private static List<ulong> VoiceCreations = new List<ulong>();

        public async Task RunAsync() {

            // Creating Configuration
            var config = new DiscordConfiguration {
                Token = Vars.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
            };

            // Creating Client
            Client = new DiscordClient(config);

            Client.UseInteractivity(new InteractivityConfiguration {
                Timeout = TimeSpan.FromMinutes(2)
            });

            // VoiceCreations channels ids
            VoiceCreations.Add(798144643275161610);
            VoiceCreations.Add(798144711869071360);
            VoiceCreations.Add(798144774262620160);
            VoiceCreations.Add(798144827932934174);
            VoiceCreations.Add(798144929967636492);


            // Bot events
            Client.Ready += OnClientReady;
            Client.Heartbeated += OnHeartbeat;
            Client.GuildAvailable += OnGuildAvailible;
            Client.MessageCreated += OnMessageCreated;
            

            // Creating Commands Configuration
            var commandsConfig = new CommandsNextConfiguration {
                StringPrefix = Vars.Prefix,
                IgnoreExtraArguments = false,
                EnableMentionPrefix = true,   // Allow to use @Friendplant as prefix
                EnableDefaultHelp = false     // Disable default help command
            };

            // Creating Commands Module
            CommandsModule = Client.UseCommandsNext(commandsConfig);

            // Registring Commands Modules
            CommandsModule.RegisterCommands<Commands.CMGeneric>(); // Generic and Standart Commands
            CommandsModule.RegisterCommands<Commands.CMMoney>();   // Money Commands
            CommandsModule.RegisterCommands<Commands.CMAdmins>();  // Admin Commands
            //////////////////////////////

            // Final Operations
            await Client.ConnectAsync(); // Enable Bot

            // Command console + infinity loop
            while(true) {
                string[] answer = Console.ReadLine().Split(' ');
                switch(answer[0]) {

                    case "close":
                    case "c":
                        File.WriteAllText(Vars.HumanityPath, JsonConvert.SerializeObject(Vars.Humanity));
                        Console.WriteLine(">> Humanity.bin was updated.");
                        File.WriteAllText(Vars.ShopPath, JsonConvert.SerializeObject(Vars.Shop));
                        Console.WriteLine(">> Shop.bin was updated.");

                        Console.WriteLine(">> Closing program...");

                        Client.DisconnectAsync().GetAwaiter().GetResult();
                        Environment.Exit(0); // Close program
                        break;

                    case "load":
                        if(answer[1].ToLower() == "humanity") {
                            Vars.Humanity = JsonConvert.DeserializeObject<Dictionary<ulong, Profile>>(File.ReadAllText(Vars.HumanityPath));
                            Console.WriteLine(">> Humanity was loaded from the file.");
                        }
                        else if(answer[1].ToLower() == "shop") {
                            Vars.Shop = JsonConvert.DeserializeObject<Dictionary<ulong, RoleItem>>(File.ReadAllText(Vars.ShopPath));
                            Console.WriteLine(">> Shop was loaded from the file.");
                        }
                        else {
                            Console.WriteLine($"Неизвестная база данных \"{answer[1]}\"");
                        }
                        break;

                    case "saveall":
                        File.WriteAllText(Vars.HumanityPath, JsonConvert.SerializeObject(Vars.Humanity));
                        Console.WriteLine(">> Humanity.json was updated.");
                        File.WriteAllText(Vars.ShopPath, JsonConvert.SerializeObject(Vars.Shop));
                        Console.WriteLine(">> Shop.json was updated.");
                        File.WriteAllText(Vars.CodesPath, JsonConvert.SerializeObject(Vars.Codes));
                        Console.WriteLine(">> Codes.json was updated.");
                        break;

                    case "save":
                    case "s":
                        if(answer[1].ToLower() == "humanity") {
                            File.WriteAllText(Vars.HumanityPath, JsonConvert.SerializeObject(Vars.Humanity));
                            Console.WriteLine(">> Humanity.json was updated.");
                        }
                        else if(answer[1].ToLower() == "shop") {
                            File.WriteAllText(Vars.ShopPath, JsonConvert.SerializeObject(Vars.Shop));
                            Console.WriteLine(">> Shop.json was updated.");
                        }
                        else if(answer[1].ToLower() == "codes") {
                            File.WriteAllText(Vars.CodesPath, JsonConvert.SerializeObject(Vars.Codes));
                            Console.WriteLine(">> Codes.json was updated.");
                        }
                        else {
                            Console.WriteLine($"Неизвестная база данных \"{answer[1]}\"");
                        }
                        break;

                    case "list":
                        if(answer[1].ToLower() == "humanity") {
                            Console.WriteLine($">>> Humanity contains {Vars.Humanity.Count} profiles");
                            Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.Black;
                            foreach(ulong b in Vars.Humanity.Keys) {
                                DiscordUser bUser = Client.GetUserAsync(b).Result;
                                Console.WriteLine($"> {bUser.Username}#{bUser.Discriminator} - {b}");
                            }
                            Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                        }
                        else if(answer[1].ToLower() == "shop") {
                            Console.WriteLine($">>> Shop contains {Vars.Humanity.Count} items");
                            Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.Black;
                            foreach(RoleItem item in Vars.Shop.Values) {

                                Console.WriteLine($"> \"{item.Name}\":\"{item.Desc}\" - {item.Cost} - {item.Id}");
                            }
                            Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                        }
                        else {
                            Console.WriteLine($"Неизвестная база данных \"{answer[1]}\"");
                        }

                        break;

                    case "radiate":
                        ulong rid = (ulong)Convert.ToInt64(answer[1]);
                        DiscordUser usr = Client.GetUserAsync(rid).Result;

                        Vars.Humanity[rid].Radioactive = !Vars.Humanity[rid].Radioactive;
                        Console.WriteLine($">> {usr.Username}#{usr.Discriminator} get a dose of radiation.");
                        break;

                    case "clear":
                        if(answer[1].ToLower() == "humanity") {
                            Vars.Humanity.Clear();
                            Console.WriteLine(">> Humanity was cleared.");
                        }
                        else if(answer[1].ToLower() == "shop") {
                            Vars.Humanity.Clear();
                            Console.WriteLine(">> Shop was cleared.");
                        }
                        else {
                            Console.WriteLine($"Неизвестная база данных \"{answer[1]}\"");
                        }

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
                    Name = "*help"
                },
                user_status: UserStatus.Online
                );

            // Voice Creations
            foreach(DiscordChannel c in Client.GetChannelAsync(798144564933951538).Result.Children) {
                VoiceCreations.Add(c.Id);
            }

            // Do Autosave
            DoAutosave = true;

            // Humanity restoring
            try {
                if(!File.Exists(Vars.HumanityPath)) {
                    File.Create(Vars.HumanityPath).Close();
                    File.WriteAllText(Vars.HumanityPath, JsonConvert.SerializeObject(Vars.Humanity));
                }

                Vars.Humanity = JsonConvert.DeserializeObject<Dictionary<ulong, Profile>>(File.ReadAllText(Vars.HumanityPath));

                if(Vars.Humanity != null) {
                    Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($">>> Humanity succesfully restored with {Vars.Humanity.Count} profiles."); Console.ResetColor();

                    Console.BackgroundColor = ConsoleColor.Magenta; Console.ForegroundColor = ConsoleColor.Black;
                    foreach(ulong key in Vars.Humanity.Keys) {

                        DiscordUser bUser = Client.GetUserAsync(key).Result;
                        Console.WriteLine($"> {bUser.Username}#{bUser.Discriminator} - {key}");
                    }
                    Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                }
                else {
                    Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine(">>> Humanity is null."); Console.ResetColor();
                }
            }
            catch(Exception exc) {
                Console.WriteLine(exc);
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($">>> Humanity.bin file is badly formed."); Console.ResetColor();
            }

            // Shop restoring
            try {
                if(!File.Exists(Vars.ShopPath)) {
                    File.Create(Vars.ShopPath).Close();
                    File.WriteAllText(Vars.ShopPath, JsonConvert.SerializeObject(Vars.Shop));
                }

                Vars.Shop = JsonConvert.DeserializeObject<Dictionary<ulong, RoleItem>>(File.ReadAllText(Vars.ShopPath));

                if(Vars.Shop != null) {
                    Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($">>> Shop succesfully restored with {Vars.Humanity.Count} items."); Console.ResetColor();

                    Console.BackgroundColor = ConsoleColor.Magenta; Console.ForegroundColor = ConsoleColor.Black;
                    foreach(RoleItem item in Vars.Shop.Values) {
                        Console.WriteLine($"> {item.Name} - \"{item.Desc}\"");
                    }
                    Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                }
                else {
                    Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine(">>> Shop is null."); Console.ResetColor();
                }
            }
            catch(Exception exc) {
                Console.WriteLine(exc);
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($">>> Shop file is badly formed."); Console.ResetColor();
            }

            // Codes Restoring
            try {
                if(!File.Exists(Vars.CodesPath)) {
                    File.Create(Vars.CodesPath).Close();
                    File.WriteAllText(Vars.CodesPath, JsonConvert.SerializeObject(Vars.Codes));
                }

                Vars.Codes = JsonConvert.DeserializeObject<Dictionary<string, PrizeCode>>(File.ReadAllText(Vars.CodesPath));

                if(Vars.Codes != null) {
                    Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($">>> Codes succesfully restored with {Vars.Humanity.Count} amount."); Console.ResetColor();

                    Console.BackgroundColor = ConsoleColor.Magenta; Console.ForegroundColor = ConsoleColor.Black;
                    foreach(PrizeCode code in Vars.Codes.Values) {
                        Console.WriteLine($"> {code.Code} - {code.Cost}");
                    }
                    Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                }
                else {
                    Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine(">>> Codes is Null."); Console.ResetColor();
                }
            }
            catch(Exception exc) {
                Console.WriteLine(exc);
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($">>> Codes file is badly formed."); Console.ResetColor();
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
            Console.WriteLine(">>> Humanity.bin autosaving...");
            File.WriteAllText(Vars.HumanityPath, JsonConvert.SerializeObject(Vars.Humanity));
            Console.WriteLine(">>> Humanity.bin succesfully saved.");

            Console.ResetColor();

            return Task.CompletedTask;
        }

        private Task OnMessageCreated(MessageCreateEventArgs e) {
            if(e.Author.IsBot) return Task.CompletedTask; // Bot ignore
            if(e.Channel.Id == 758374102649667615) return Task.CompletedTask; // Ignore spam channel

            new Profile(e.Author);

            if(Vars.Humanity[e.Author.Id].Level.AddExp()) { // Return true, if levelUp

                int blesk = Convert.ToInt32(5 * 0.5 * Vars.Humanity[e.Author.Id].Level.Amount); // Prize sparkles
                Vars.Humanity[e.Author.Id].Balance.Transfer(blesk, new HistoryElement(Vars.Humanity[e.Author.Id].Balance, "+"+blesk, "Новый уровень"), false);

                if(!Vars.Humanity[e.Author.Id].Level.Muted) e.Channel.SendMessageAsync(
                    $"> :tada: Поздравляю, {e.Author.Mention}, ты достиг `{Vars.Humanity[e.Author.Id].Level.Amount}` уровня!\n> Ты получаешь `{blesk}`{Vars.Emoji["sparkle"]} блестяшек!");
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
