using System;
using Telegram.Bot;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading;
using System.Data.SqlClient;
using Telegram.Bot.Types.ReplyMarkups;
using System.Data;

namespace PizzeriaBot
{
    

    class StartUp
    {
        static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kirill\Desktop\SuperTelegramBot\SuperTelegramBot\Statistic.mdf;Integrated Security=True";
        static SqlConnection sql = new SqlConnection(connectionString);
        static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient("5045636554:AAHkHBI6LmECQQUo3rrcjmwBlj1Yg5YjQR0");
            var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } 
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            
            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        public static int GetStep(Update update)

        {
            sql.Open();
                SqlCommand command = new SqlCommand($"Select Step From StatisticOfUser Where Id_User = '{update.Message.Chat.Id}'", sql);
                SqlDataReader reader = command.ExecuteReader();
                reader.ReadAsync();
                var step = Convert.ToInt32(reader[0]);
                reader.Close();
            sql.Close();
            return step;
        }
        public static void SetStep(Update update, int step)
        {
                sql.Open();
                    SqlCommand command = new SqlCommand($"Update StatisticOfUser Set Step = '{step}' where Id_User = '{update.Message.Chat.Id}'", sql);
                    command.ExecuteNonQueryAsync();
                    sql.Close();
        }


        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message)
                return;

            if (update.Message!.Type != MessageType.Text)
                return;
            var chatId = update.Message.Chat.Id;
            var message = update.Message;

            var keyboard = new ReplyKeyboardMarkup(new KeyboardButton(""))
            { Keyboard = new[] {new[] { new KeyboardButton("Дальше➡️"),},}, ResizeKeyboard = true,};


            if (message.Text == "/start") // "встречающие" сообщения
            {
                if (sql.State == ConnectionState.Closed)
                {
                    sql.Open();
                    SqlCommand delete = new SqlCommand($"delete StatisticOfUser Where Id_User ='{update.Message.Chat.Id}' ", sql);
                    await delete.ExecuteNonQueryAsync();
                    SqlCommand command = new SqlCommand($"insert into StatisticOfUser (Id_User,Step,User_pizzeria_name) values ('{update.Message.Chat.Id}','1', N'й')", sql);
                    await command.ExecuteNonQueryAsync();
                     sql.Close();
                }
                Start.startMess(update);
            }
            if (message.Text.Contains("Пиццерия") && StartUp.GetStep(update)==1 && message.Text.Remove(0, 8)!="") // ожидание названия
            {
                Start.waitMessage(update, keyboard);
                if (sql.State == ConnectionState.Closed)
                {
                    sql.Open();
                    SqlCommand command = new SqlCommand($"Update StatisticOfUser Set User_pizzeria_name = N'{update.Message.Text.Remove(0, 8)}' where Id_User = '{update.Message.Chat.Id}'", sql);
                    await command.ExecuteNonQueryAsync();
                    sql.Close();
                }
                
            }

            if (message.Text == "Дальше➡️") // кнопка смены блоков обучения
            {
                Education.startEducation(update, keyboard); 
            }
            if (message.Text == "🍕Заказы") // раздел заказов
            {
                Orders.orderAsync(update);
            }
            if(message.Text == "❌Отказать") // отказ от облуживания клиента
            {
                keyboard = new ReplyKeyboardMarkup(new KeyboardButton(""))
                {Keyboard = new[]{new[]{
                                        new KeyboardButton("📊Статистика"),
                                        new KeyboardButton("⬆️Улучшение"),
                                        new KeyboardButton("🍕Заказы")},}, 
                                        ResizeKeyboard = true,
                };

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Нехорошо. Клиент ушел недовольный :(",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
            if (message.Text == "✅Приготовить")//обработка заказа
                                                // счет денег в БД
            {
                 keyboard = new ReplyKeyboardMarkup(new KeyboardButton(""))
                {Keyboard = new[] { new[] { new KeyboardButton("Greate"),},},ResizeKeyboard = true,};

                String nameVisitor = "Пусто";
                String nameOrder = "Пусто";
                Int64 price = 0;
                await botClient.SendTextMessageAsync(
                      chatId: chatId,
                      text: "Готовим",
                      cancellationToken: cancellationToken);

                sql.Open();
                    if (Orders.NumOrder == Stade.countRecords) Orders.NumOrder = 2;

                    SqlCommand commandName = new SqlCommand($"Select Name From View_order Where Id = '{Orders.NumOrder-1}'", sql);
                    SqlCommand commandOrderName = new SqlCommand($"Select Name_order From View_order Where Id = '{Orders.NumOrder-1}'", sql);
                    SqlCommand commandPrice = new SqlCommand($"Select Price From View_order Where Id = '{Orders.NumOrder-1}'", sql);

                    SqlDataReader readerName = commandName.ExecuteReader();
                    await readerName.ReadAsync();
                        nameVisitor = Convert.ToString(readerName[0]);
                        readerName.Close();

                    SqlDataReader readerOrder = commandOrderName.ExecuteReader();
                    await readerOrder.ReadAsync();
                        nameOrder = Convert.ToString(readerOrder[0]);
                        readerOrder.Close();

                    SqlDataReader readerPrice = commandPrice.ExecuteReader();
                    await readerPrice.ReadAsync();
                        price = Convert.ToInt64(readerPrice[0]);
                        readerPrice.Close();
                
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Заказ готов. Ты заработал " + price + "$",
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken);

                    Int64 money = 0;
                    SqlCommand Money = new SqlCommand($"Select Money From StatisticOfUser Where Id_User = '{update.Message.Chat.Id}'", sql);
                    SqlDataReader moneyReader = Money.ExecuteReader();
                    await moneyReader.ReadAsync();
                        money = Convert.ToInt64(moneyReader[0]);
                        moneyReader.Close();

                    SqlCommand command = new SqlCommand($"Update StatisticOfUser Set Money = N'{money+price}' where Id_User = '{update.Message.Chat.Id}'", sql);
                    await command.ExecuteNonQueryAsync();
                sql.Close();
            }
            if ( message.Text == "Greate")//возврат в главное меню
            {
                keyboard = new ReplyKeyboardMarkup(new KeyboardButton(""))
                {
                    Keyboard = new[]{new[]{
                                        new KeyboardButton("📊Статистика"),
                                        new KeyboardButton("⬆️Улучшение"),
                                        new KeyboardButton("🍕Заказы")},},
                    ResizeKeyboard = true,
                };

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Дела идут неплохо)",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }

            if (update.Message.Text == "📊Статистика") // раздел статистики определенного пользователя
                                                       // выбор данных из БД
            {
                String pizzeriaName = "Пусто";
                Int64 money = 0;
                if (sql.State == ConnectionState.Closed)
                {

                 sql.Open();
                    SqlCommand commandPizzaName = new SqlCommand($"Select User_pizzeria_name From StatisticOfUser Where Id_User = '{chatId}'", sql);
                    SqlDataReader readerName = commandPizzaName.ExecuteReader();
                    await readerName.ReadAsync();
                        pizzeriaName = Convert.ToString(readerName[0]);
                        readerName.Close();

                    SqlCommand commandMoney = new SqlCommand($"Select Money From StatisticOfUser Where Id_User = '{chatId}'", sql);
                    SqlDataReader readerMoney = commandMoney.ExecuteReader();

                        await readerMoney.ReadAsync();
                        money = Convert.ToInt64(readerMoney[0]);
                        readerMoney.Close();
                    
                    SqlCommand commandStade = new SqlCommand($"Select StadePizzeria From StatisticOfUser Where Id_User = '{chatId}'", sql);
                    SqlDataReader readerStade = commandStade.ExecuteReader();
                        await readerStade.ReadAsync();
                        int stade = Convert.ToInt32(readerStade[0]);
                        readerStade.Close();
                 sql.Close();

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "🍕 Название твоей пиццерии: " + pizzeriaName + "\n💵 Честно заработааных денег : " + money + "$ \n⬆️ Уровень Пиццерии: " + stade + "\n✅ Молодец так держать!",
                    cancellationToken: cancellationToken);
}
          
            }

            if (update.Message.Text == "⬆️Улучшение")// реализация логики улучшения пиццерии
            {
                sql.Open();
                    SqlCommand commandStade = new SqlCommand($"Select StadePizzeria From StatisticOfUser Where Id_User = '{chatId}'", sql);
                    SqlDataReader readerStade = commandStade.ExecuteReader();
                    await readerStade.ReadAsync();
                        int stade = Convert.ToInt32(readerStade[0]);
                        readerStade.Close();
                sql.Close();


                await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "Твоя пиццерия имеет " + stade + " уровень",
                       cancellationToken: cancellationToken);
                var keyboardUp = new ReplyKeyboardMarkup(new KeyboardButton(""))
                {Keyboard = new[]{new[]{new KeyboardButton("⬆️Улучшить"),},},ResizeKeyboard = true,};
                if (stade == 0)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Чтобы улучшить твою пиццерию понадобиться 400$",
                        replyMarkup: keyboardUp,
                        cancellationToken: cancellationToken);
                }

                if (stade == 1)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Чтобы улучшить твою пиццерию понадобиться 1000$",
                        replyMarkup: keyboardUp,
                        cancellationToken: cancellationToken);
                }

                if (stade == 2)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Чтобы улучшить твою пиццерию понадобиться 5000$ Это будет максимальный уровень",
                        replyMarkup: keyboardUp,
                        cancellationToken: cancellationToken);
                }
            }
            if (update.Message.Text == "⬆️Улучшить")
            {
                sql.Open();
                    SqlCommand commandMoney = new SqlCommand($"Select Money From StatisticOfUser Where Id_User = '{chatId}'", sql);
                    SqlDataReader readerMoney = commandMoney.ExecuteReader();
                    await readerMoney.ReadAsync();
                        var money = Convert.ToInt32(readerMoney[0]);
                        readerMoney.Close();
              
                    SqlCommand commandStade = new SqlCommand($"Select StadePizzeria From StatisticOfUser Where Id_User = '{chatId}'", sql);
                    SqlDataReader readerStade = commandStade.ExecuteReader();
                    await readerStade.ReadAsync();
                        int stade = Convert.ToInt32(readerStade[0]);
                        readerStade.Close();
                sql.Close();
                if (money >= 400 &&  stade == 0)
                {
                    Stade.UpStadeAsync(update, 1);// добавление улучшений 1 уровня
                    Stade.countRecords = 13;
                    sql.Open();
                        SqlCommand command = new SqlCommand($"Update StatisticOfUser Set Money = N'{money - 400}' where Id_User = '{update.Message.Chat.Id}'", sql);
                        await command.ExecuteNonQueryAsync();
                   
                        SqlCommand commandUP = new SqlCommand($"Update StatisticOfUser Set StadePizzeria = N'{1}' where Id_User = '{update.Message.Chat.Id}'", sql);
                        await commandUP.ExecuteNonQueryAsync(); //Выполняем команду
                    sql.Close();
                    keyboard = new ReplyKeyboardMarkup(new KeyboardButton(""))
                    {
                        Keyboard = new[]{new[]{
                                        new KeyboardButton("📊Статистика"),
                                        new KeyboardButton("⬆️Улучшение"),
                                        new KeyboardButton("🍕Заказы")},},
                        ResizeKeyboard = true,
                    };
                    await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "Супер! Количество клиентов увелилось, в меню добавились новые позиции\n ",
                       replyMarkup: keyboard,
                       cancellationToken: cancellationToken);
                }
                else if (money >= 1000 && stade == 1)
                {
                    Stade.UpStadeAsync(update, 2);// добавление улучшений 2 уровня
                    Stade.countRecords = 18;
                    sql.Open();
                        SqlCommand command = new SqlCommand($"Update StatisticOfUser Set Money = N'{money - 1000}' where Id_User = '{update.Message.Chat.Id}'", sql);
                        await command.ExecuteNonQueryAsync(); //Выполняем команду
                   
                        SqlCommand commandUP = new SqlCommand($"Update StatisticOfUser Set StadePizzeria = N'{2}' where Id_User = '{update.Message.Chat.Id}'", sql);
                        await commandUP.ExecuteNonQueryAsync(); //Выполняем команду
                    sql.Close();

                    keyboard = new ReplyKeyboardMarkup(new KeyboardButton(""))
                    {
                        Keyboard = new[]{new[]{
                                        new KeyboardButton("📊Статистика"),
                                        new KeyboardButton("⬆️Улучшение"),
                                        new KeyboardButton("🍕Заказы")},},
                        ResizeKeyboard = true,
                    };

                    await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "Супер! Количество клиентов увелилось, в меню добавились новые позиции\n ",
                       replyMarkup: keyboard,
                       cancellationToken: cancellationToken);
                }
                else if (money >=5000 && stade == 2)
                {

                    keyboard = new ReplyKeyboardMarkup(new KeyboardButton(""))
                    {
                        Keyboard = new[]{new[]{
                                        new KeyboardButton("📊Статистика"),
                                        new KeyboardButton("⬆️Улучшение"),
                                        new KeyboardButton("🍕Заказы")},},
                        ResizeKeyboard = true,
                    };

                    Stade.UpStadeAsync(update, 3);// добавление улучшений 3 уровня
                    sql.Open();
                        SqlCommand commandUP = new SqlCommand($"Update StatisticOfUser Set StadePizzeria = N'{3}' where Id_User = '{update.Message.Chat.Id}'", sql);
                        await commandUP.ExecuteNonQueryAsync(); //Выполняем команду
                    sql.Close();

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                       text: "Супер! Мы увеличили цены. Можем себе позволить) ",
                       replyMarkup: keyboard,
                       cancellationToken: cancellationToken);
                }
                else if (stade == 3)
                {
                    await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "Извини это максимальный уровень. Молодец! ",
                       replyMarkup: keyboard,
                       cancellationToken: cancellationToken);
                }
                else
                {
                    keyboard = new ReplyKeyboardMarkup(new KeyboardButton(""))
                    {
                        Keyboard = new[]{new[]{
                                        new KeyboardButton("📊Статистика"),
                                        new KeyboardButton("⬆️Улучшение"),
                                        new KeyboardButton("🍕Заказы")},},
                        ResizeKeyboard = true,
                    };

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                       text: "Денег пока не хвататает",
                       replyMarkup: keyboard,
                       cancellationToken: cancellationToken);
                }
            }

            if (update.Type == UpdateType.Message)
            {
                sql.Open();
                    SqlCommand commandMoney = new SqlCommand($"Select Money From StatisticOfUser Where Id_User = '{chatId}'", sql);
                    SqlDataReader readerMoney = commandMoney.ExecuteReader();
                    await readerMoney.ReadAsync();
                        var money = Convert.ToInt32(readerMoney[0]);
                        readerMoney.Close();
               
                    SqlCommand commandWin = new SqlCommand($"Select Win From StatisticOfUser Where Id_User = '{chatId}'", sql);
                    SqlDataReader readerWin = commandWin.ExecuteReader();
                    await readerWin.ReadAsync();
                        var Win = Convert.ToInt32(readerWin[0]);
                        readerWin.Close();
                sql.Close();
                if (money < 100000 && Win == 0) { } else if (Win != 1) // окончание игры
                {
                    sql.Open();
                        SqlCommand command = new SqlCommand($"Update StatisticOfUser Set Money = N'{money - 100000}' where Id_User = '{update.Message.Chat.Id}'", sql);
                        await command.ExecuteNonQueryAsync();
                   
                        SqlCommand commandW = new SqlCommand($"Update StatisticOfUser Set Win = N'{1}' where Id_User = '{update.Message.Chat.Id}'", sql);
                        await commandW.ExecuteNonQueryAsync();
                    sql.Close();

                    await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "Поздравляю ты выиграл ты сделал эту пиццерию известной на весь город. Долг упалачен значит она по праву твоя. Было приятно иметь с тобой дело. Удачи! И до встречи!",
                       cancellationToken: cancellationToken);
                }
            }
            Console.WriteLine($"Received a '{message.Text}' message in chat {chatId}.");

        }
        static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
