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

            // Создание конфигурации
            var config = new DiscordConfiguration {
                Token = Vars.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true,
            };

            // Создаем Клиент бота
            Client = new DiscordClient(config);

            Client.UseInteractivity(new InteractivityConfiguration {
                Timeout = TimeSpan.FromMinutes(2)
            });

            // События бота
            Client.Ready += OnClientReady;
            Client.Heartbeated += OnHeartbeat;
            Client.GuildAvailable += OnGuildAvailible;
            Client.MessageCreated += OnMessageCreated;
            

            // Создаем конфигурацию команд
            var commandsConfig = new CommandsNextConfiguration {
                StringPrefix = Vars.Prefix,
                IgnoreExtraArguments = false,
                EnableMentionPrefix = true,   // Позволяет использовать пинг как префикс команд
                EnableDefaultHelp = false     // Выключает стандартную команду help
            };

            // Создаем модуль команд
            CommandsModule = Client.UseCommandsNext(commandsConfig);

            // Регистрация Команд
            CommandsModule.RegisterCommands<Commands.CMGeneric>(); // Стандартные команды
            CommandsModule.RegisterCommands<Commands.CMMoney>();   // Команды связанные с деньгами
            CommandsModule.RegisterCommands<Commands.CMAdmins>();  // Команды админов
            /////////////////////

            // Включаем бота, ало
            await Client.ConnectAsync();

            // Консоль для команд + вечный цикл чтобы бот не выключился
            while(true) {
                string[] answer = Console.ReadLine().Split(' ');
                switch(answer[0]) {

                    // Выключить бота, сохранив все БД
                    case "close":
                    case "c":
                        File.WriteAllText(Vars.HumanityPath, JsonConvert.SerializeObject(Vars.Humanity, Newtonsoft.Json.Formatting.Indented));
                        Console.WriteLine(">> Humanity.json was updated.");
                        File.WriteAllText(Vars.ShopPath, JsonConvert.SerializeObject(Vars.Shop, Newtonsoft.Json.Formatting.Indented));
                        Console.WriteLine(">> Shop.json was updated.");

                        Console.WriteLine(">> Closing program...");

                        Client.DisconnectAsync().GetAwaiter().GetResult();
                        Environment.Exit(0); // Закрывает программу
                        break;
                    
                    // Загрузить БД из файла
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

                    // Сохранить ВСЕ БД в файлы
                    case "saveall":
                        File.WriteAllText(Vars.HumanityPath, JsonConvert.SerializeObject(Vars.Humanity));
                        Console.WriteLine(">> Humanity.json was updated.");
                        File.WriteAllText(Vars.ShopPath, JsonConvert.SerializeObject(Vars.Shop));
                        Console.WriteLine(">> Shop.json was updated.");
                        File.WriteAllText(Vars.CodesPath, JsonConvert.SerializeObject(Vars.Codes));
                        Console.WriteLine(">> Codes.json was updated.");
                        break;

                    // Сохранить БД в файл
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

                    // Выводит список элементов в БД
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

                    // Выдает дозу радиации (админку) пользователю
                    case "radiate":
                        ulong rid = (ulong)Convert.ToInt64(answer[1]);
                        DiscordUser usr = Client.GetUserAsync(rid).Result;

                        Vars.Humanity[rid].Radioactive = !Vars.Humanity[rid].Radioactive;
                        Console.WriteLine($">> {usr.Username}#{usr.Discriminator} get a dose of radiation.");
                        break;

                    // Обнулить БД
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

                    // Включает/выключает автосохранение БД
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

            // Установка активности бота
            Client.UpdateStatusAsync(
                game: new DiscordGame {
                    Name = "*help"
                },
                user_status: UserStatus.Online
                );

            // Голосовые каналы - создателя  (Пока не используется)
            // foreach(DiscordChannel c in Client.GetChannelAsync(798144564933951538).Result.Children) {
            //    VoiceCreations.Add(c.Id);
            // }

            // Включение Автосохранения
            DoAutosave = true;

            // Восстановление Humanity
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

            // Восстановление Shop
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

            // Восстановление Codes
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

            // Конец
            return Task.CompletedTask;
        }

        private Task OnHeartbeat(HeartbeatEventArgs e) {

            // Если Автосохранение выключено - завершить
            if(!DoAutosave) return Task.CompletedTask;

            if(AutosaveIn < AutosaveEveryHeartbeat) {
                AutosaveIn++;
                return Task.CompletedTask;
            }
            else {
                AutosaveIn = 0;
            }

            // Само сохранение
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(">>> Humanity.bin autosaving...");
            File.WriteAllText(Vars.HumanityPath, JsonConvert.SerializeObject(Vars.Humanity));
            Console.WriteLine(">>> Humanity.bin succesfully saved.");
            Console.ResetColor();

            // Конец
            return Task.CompletedTask;
        }

        private Task OnMessageCreated(MessageCreateEventArgs e) {
            if(e.Author.IsBot) return Task.CompletedTask; // Игнорировать сообщеения от ботов
            if(e.Channel.Id == 758374102649667615) return Task.CompletedTask; // Игнорирование канала для спама

            new Profile(e.Author);

            if(Vars.Humanity[e.Author.Id].Level.AddExp()) { // Активируется, если у чела новый уровень

                int blesk = Convert.ToInt32(5 * 0.5 * Vars.Humanity[e.Author.Id].Level.Amount); // Вычисление денег для награды
                Vars.Humanity[e.Author.Id].Balance.Transfer(blesk, false, $"+{blesk}", "Новый уровень"); // Начисление

                if(!Vars.Humanity[e.Author.Id].Level.Muted) e.Channel.SendMessageAsync( // Активируется если уведомления включены
                    $"> :tada: Поздравляю, {e.Author.Mention}, ты достиг `{Vars.Humanity[e.Author.Id].Level.Amount}` уровня!\n> Ты получаешь `{blesk}`{Vars.Emoji["sparkle"]} блестяшек!");
            }

            // Конец
            return Task.CompletedTask;
        }

        private Task OnGuildAvailible(GuildCreateEventArgs e) {

            if(e.Guild.Id == 797103909424332811) { // Добавление эмодзи с Friendplant case
                foreach(DiscordEmoji emoji in e.Guild.Emojis) {
                    Vars.Emoji[emoji.Name] = $"<:{emoji.Name}:{emoji.Id}>";
                }
            }

            // Конец
            return Task.CompletedTask;
        }

    }
}
