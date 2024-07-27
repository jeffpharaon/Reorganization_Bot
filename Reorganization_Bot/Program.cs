using Terminal.Gui;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Reorganization_Bot
{
    internal partial class Bot
    {
        private static HelpList helpList = new HelpList();
        private static DataBase db = new DataBase();
        private static Repository repository = new Repository();

        private static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();
        private static Dictionary<long, (string Login, string Role)> authorizedUsers = new Dictionary<long, (string Login, string Role)>();
        private static List<string> errorLog = new List<string>();

        private static string listUser; // сообщение пользователя
        private static string helpText = helpList.showPeople; // список команд

        private static bool isCheck = false; //проверка на авторизацию (авторизовался ли уже пользователь)
        private static bool isAdmin = false; //проверка на админа
        private static bool isUser = false; //проверка на юзера

        public class UserState
        {
            public int Step { get; set; }
            public string Name { get; set; }
            public string Order { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }
            public string Job { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }

        private static void Main(string[] args)
        {
            db.Connection("JEFFPHARAON\\SQLEXPRESS", "Reorganization"); //инициализация базы данных
            var client = new TelegramBotClient(""); //api-ключ является конфиденциальной информацией
            client.StartReceiving(Update, Error);
            Console.ReadLine();
        }

        private async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;
            if (message == null && callbackQuery == null) return;

            try
            {
                if (callbackQuery != null)
                {
                    var data = callbackQuery.Data;
                    if (data == "HelpButton")
                    {
                        if (isCheck == false && isAdmin == false && isUser == false) helpText = helpList.showPeople;
                        else if (isCheck == true && isAdmin == true) helpText = helpList.showAdmin;
                        else if (isCheck == true && isUser == true) helpText = helpList.showUser;
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, helpText);
                        return;
                    }
                    else if (data == "LoginButton")
                    {
                        var chatId = callbackQuery.Message.Chat.Id;
                        if (authorizedUsers.ContainsKey(chatId))
                        {
                            await botClient.SendTextMessageAsync(chatId, "✅ Вы уже авторизованы!");
                            return;
                        }
                        userStates[chatId] = new UserState { Step = 4 };
                        await botClient.SendTextMessageAsync(chatId, "✒️ Введите логин");
                        return;
                    }
                }
                else
                {
                    var chatId = message.Chat.Id; // Определение chatId
                    //отображение  пользовательских сообщений
                    listUser = $"{message.Chat.Username ?? message.Chat.FirstName ?? "Unknown"} | {message.Text}";

                    if (message.Text.ToLower().Contains("/start"))
                    {
                        await botClient.SendTextMessageAsync(chatId, "🤖 Бот центрального управления Реорганизации " +
                            "приветствует Вас, введите команду /help чтобы увидеть мой функционал");
                        return;
                    }

                    else if (message.Text.ToLower().Contains("/help"))
                    {
                        if (!isCheck)
                        {
                            var keyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("❓Помощь", "HelpButton"),
                                    InlineKeyboardButton.WithCallbackData("👤Войти", "LoginButton")
                                }
                            });
                            await botClient.SendTextMessageAsync(chatId, "Выберите действие:", replyMarkup: keyboard);
                        }
                        else
                        {
                            if (isAdmin) helpText = helpList.showAdmin;
                            else if (isUser) helpText = helpList.showUser;
                            await botClient.SendTextMessageAsync(chatId, helpText);
                        }
                        return;
                    }

                    else if (message.Text.ToLower().Contains("/rules"))
                    {
                        await botClient.SendTextMessageAsync(chatId, repository.rules);
                        return;
                    }

                    else if (message.Text.ToLower().Contains("/jobs"))
                    {
                        var jobs = await db.GetJobsAsync();
                        if (jobs != null && jobs.Any())
                        {
                            var jobMessages = jobs.Select(job => $"Вакансия: {job.JobTitle}\nОписание: {job.Description}");
                            var response = string.Join("\n\n", jobMessages);
                            await botClient.SendTextMessageAsync(chatId, "⚒️ВАКАНСИИ⚒️\r\n\r\n" + response);
                        }
                        else await botClient.SendTextMessageAsync(chatId, "❌ Нет доступных вакансий.");
                        return;
                    }

                    else if (message.Text.ToLower().Contains("/statement"))
                    {
                        if (!isCheck)
                        {
                            userStates[chatId] = new UserState { Step = 1 };
                            await botClient.SendTextMessageAsync(chatId, "🔤 Введите ваше имя:");
                        }
                        else await botClient.SendTextMessageAsync(chatId, "⚠️ Вы уже числитесь в организации");
                        return;
                    }

                    else if (message.Text.ToLower().Contains("/sign"))
                    {
                        if (authorizedUsers.ContainsKey(chatId))
                        {
                            await botClient.SendTextMessageAsync(chatId, "✅ Вы уже авторизованы!");
                            return;
                        }
                        userStates[chatId] = new UserState { Step = 4 };
                        await botClient.SendTextMessageAsync(chatId, "✒️ Введите логин");
                        return;
                    }

                    if (userStates.ContainsKey(chatId))
                    {
                        var userState = userStates[chatId];
                        switch (userState.Step)
                        {
                            case 1:
                                userState.Name = message.Text.Trim();
                                userState.Step = 2;
                                await botClient.SendTextMessageAsync(chatId, "📎 Введите ссылку на ваш телеграмм или ВК:");
                                return;
                            case 2:
                                userState.Link = message.Text.Trim();
                                userState.Step = 3;
                                await botClient.SendTextMessageAsync(chatId, "⚒ Укажите желаемую вакансию:");
                                return;
                            case 3:
                                userState.Job = message.Text.Trim();
                                await db.InsertApplicationAsync(userState.Name, userState.Link, userState.Job);
                                await botClient.SendTextMessageAsync(chatId, "✅ Ваше заявление принято! " +
                                    "Заявка будет рассмотрена в течении 48 часов, ожидайте уведомления!");
                                userStates.Remove(chatId);
                                return;

                            //ЛОГИКА АВТОРИЗАЦИИ
                            case 4:
                                userState.Login = message.Text.Trim();
                                userState.Step = 5;
                                await botClient.SendTextMessageAsync(chatId, "✒️ Введите пароль");
                                return;
                            case 5:
                                userState.Password = message.Text.Trim();
                                var user = await db.CheckUserCredentialsAsync(userState.Login, userState.Password);
                                if (user != default)
                                {
                                    authorizedUsers[chatId] = user; // Сохраняем логин и роль пользователя
                                    await botClient.SendTextMessageAsync(chatId, "✅ Авторизация успешна!");
                                    isCheck = true;
                                    isAdmin = user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
                                    isUser = user.Role.Equals("user", StringComparison.OrdinalIgnoreCase);

                                    if (isAdmin) helpText = helpList.showAdmin;
                                    else if (isUser) helpText = helpList.showUser;
                                }
                                else await botClient.SendTextMessageAsync(chatId, "❌ Неверный логин или пароль.");
                                userStates.Remove(chatId);
                                return;

                            // ЛОГИКА ДОБАВЛЕНИЯ ПРИКАЗА
                            case 6:
                                userState.Name = message.Text.Trim();
                                userState.Step = 7;
                                await botClient.SendTextMessageAsync(chatId, "Введите название приказа:");
                                return;
                            case 7:
                                userState.Order = message.Text.Trim();
                                userState.Step = 8;
                                await botClient.SendTextMessageAsync(chatId, "Введите описание и суть приказа:");
                                return;
                            case 8:
                                userState.Description = message.Text.Trim();
                                var leader = await db.GetLeaderByLoginAsync(authorizedUsers[chatId].Login);
                                var link = await db.GetLinkByNameAsync(userState.Name);
                                await db.InsertOrderAsync(leader, userState.Name, userState.Order, userState.Description, link);
                                if (!string.IsNullOrEmpty(link))
                                {
                                    await botClient.SendTextMessageAsync(chatId, "✅ Приказ добавлен и уведомление отправлено!");
                                    // Отправка сообщения по имени пользователя
                                    if (link.StartsWith("https://t.me/"))
                                    {
                                        var username = link.Replace("https://t.me/", "");
                                        try
                                        {
                                            await botClient.SendTextMessageAsync($"@{username}", "Внимание, у вас новый приказ, введите /orders чтобы посмотреть все ваши приказы");
                                        }
                                        catch (Exception ex)
                                        {
                                            await botClient.SendTextMessageAsync(chatId, $"✅ Приказ добавлен, но не удалось отправить уведомление. Ошибка: {ex.Message}");
                                        }
                                    }
                                }
                                else await botClient.SendTextMessageAsync(chatId, "✅ Приказ добавлен, но не удалось отправить уведомление.");
                                userStates.Remove(chatId);
                                return;
                        }
                    }

                    else if (message.Text.ToLower().Contains("/info"))
                    {
                        await botClient.SendTextMessageAsync(chatId, repository.info);
                        return;
                    }

                    else if (message.Text.ToLower().Contains("/structure"))
                    {
                        var members = await db.GetAllMembersAsync();
                        if (members != null && members.Any())
                        {
                            var memberMessages = members.Select(member => $"Имя: {member.Name}\nСсылка: {member.Link}");
                            var response = string.Join("\n\n", memberMessages);
                            await botClient.SendTextMessageAsync(chatId, "📋 ЧЛЕНЫ ОРГАНИЗАЦИИ 📋\r\n\r\n" + response);
                        }
                        else await botClient.SendTextMessageAsync(chatId, "❌ Нет данных о членах организации.");
                        return;
                    }

                    // Обработка команд в зависимости от роли пользователя
                    if (authorizedUsers.ContainsKey(chatId))
                    {
                        var userRole = authorizedUsers[chatId].Role.Trim(); // Удаляем лишние пробелы

                        if (userRole.Equals("admin", StringComparison.OrdinalIgnoreCase))
                        {
                            isAdmin = true;
                            isUser = false;
                            await HandleAdminCommands(botClient, message, chatId);
                        }

                        else if (userRole.Equals("user", StringComparison.OrdinalIgnoreCase))
                        {
                            isUser = true;
                            isAdmin = false;
                            await HandleUserCommands(botClient, message, chatId);
                        }
                    }
                    else await botClient.SendTextMessageAsync(chatId, "❌ Извините, но я не знаю такой команды");
                }
            }
            catch (Exception ex)
            {
                string erorrExp = $"Error at {DateTime.Now} : {ex.Message}";
                errorLog.Add(erorrExp);
            }
        }

        //КОМАНДЫ ПОСЛЕ АВТОРИЗАЦИИ

        //метод администратоар 
        private static async Task HandleAdminCommands(ITelegramBotClient botClient, Message message, long chatId)
        {
            if (message.Text.ToLower().Contains("/account"))
            {
                var userInfo = await db.GetUserInfoAsync(authorizedUsers[chatId].Login);
                if (userInfo != default)
                {
                    var response = $"🟢 АККАУНТ \r\n\r\nИмя: {userInfo.Name}\nЛогин: {userInfo.Login}\nРоль: {userInfo.Role}";
                    await botClient.SendTextMessageAsync(chatId, response);
                }
                else await botClient.SendTextMessageAsync(chatId, "❌ Не удалось получить информацию о пользователе.");
                return;
            }

            else if (message.Text.ToLower().Contains("/help"))
            {
                helpText = helpList.showAdmin;
                await botClient.SendTextMessageAsync(chatId, helpText);
                return;
            }

            else if (message.Text.ToLower().Contains("/add_order"))
            {
                userStates[chatId] = new UserState { Step = 6 };
                await botClient.SendTextMessageAsync(chatId, "Введите имя члена организации, которому хотите назначить приказ:");
                return;
            }

            else await botClient.SendTextMessageAsync(chatId, "❌ Извините, но я не знаю такой команды");
        }

        //метод юзера
        private static async Task HandleUserCommands(ITelegramBotClient botClient, Message message, long chatId)
        {
            if (message.Text.ToLower().Contains("/account"))
            {
                var userInfo = await db.GetUserInfoAsync(authorizedUsers[chatId].Login);
                if (userInfo != default)
                {
                    var response = $"🔵 АККАУНТ \r\n\r\nИмя: {userInfo.Name}\nЛогин: {userInfo.Login}\nРоль: {userInfo.Role}";
                    await botClient.SendTextMessageAsync(chatId, response);
                    return;
                }
                else await botClient.SendTextMessageAsync(chatId, "❌ Не удалось получить информацию о пользователе.");
            }

            else if (message.Text.ToLower().Contains("/help"))
            {
                helpText = helpList.showUser;
                await botClient.SendTextMessageAsync(chatId, helpText);
                return;
            }

            else if (message.Text.ToLower().Contains("/orders"))
            {
                var userName = authorizedUsers[chatId].Login.Trim();
                var userInfo = await db.GetUserInfoAsync(userName);
                var orders = await db.GetOrdersByNameAsync(userInfo.Name.Trim());
                if (orders != null && orders.Any())
                {
                    var orderMessages = orders.Select(order => $"Приказ: {order.Orders}\nОписание: {order.Description}\nЛидер: {order.Leader}");
                    var response = string.Join("\n\n", orderMessages);
                    await botClient.SendTextMessageAsync(chatId, "📜 ПРИКАЗЫ 📜\r\n\r\n" + response);
                }
                else await botClient.SendTextMessageAsync(chatId, "❌ У вас нет приказов.");
                return;
            }

            else await botClient.SendTextMessageAsync(chatId, "❌ Извините, но я не знаю такой команды");
        }

        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            string errorMessage = $"Error at {DateTime.Now}: {arg2.Message}";
            errorLog.Add(errorMessage);
            Console.WriteLine(errorMessage); // Логирование ошибок (переделать под библиотеку Terminal.GUI)
            return Task.CompletedTask;
        }
    }
}
