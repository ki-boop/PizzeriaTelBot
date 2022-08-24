using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PizzeriaBot
{
    class Orders
       
    {
        public static int NumOrder = 1;
        static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kirill\Desktop\SuperTelegramBot\SuperTelegramBot\Statistic.mdf;Integrated Security=True";
        static SqlConnection sql = new SqlConnection(connectionString);
        public static async void orderAsync(Update update)
        {
            var botClient = new TelegramBotClient("5045636554:AAHkHBI6LmECQQUo3rrcjmwBlj1Yg5YjQR0");
            var cts = new CancellationTokenSource();
            var chatId = update.Message.Chat.Id;

            var keyboard = new ReplyKeyboardMarkup(new KeyboardButton(""))
            {
                Keyboard = new[]{new[]{
                                        new KeyboardButton("❌Отказать"),
                                        new KeyboardButton("✅Приготовить")},},
                ResizeKeyboard = true,
            };

            if (NumOrder < Stade.countRecords)
            {
               await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Новый заказ",
                    replyMarkup: keyboard,
                    cancellationToken: cts.Token);

               String nameVisitor = "Пусто";
               String nameOrder = "Пусто";
               Int64 price = 0;
               if (sql.State == ConnectionState.Closed)
               {
                    sql.Open();

                        SqlCommand commandName = new SqlCommand($"Select Name From View_order Where Id = '{NumOrder}'", sql);
                        SqlCommand commandOrderName = new SqlCommand($"Select Name_order From View_order Where Id = '{NumOrder}'", sql);
                        SqlCommand commandPrice = new SqlCommand($"Select Price From View_order Where Id = '{NumOrder}'", sql);

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
                    sql.Close();
                    NumOrder++;
               }

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: nameVisitor + " заказал(а) " + nameOrder + ". За этот заказ ты получишь " + price + "$",
                    cancellationToken: cts.Token);
            }
             else {
                NumOrder = 1;
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
                    text: "Заказов нет",
                    replyMarkup: keyboard,
                    cancellationToken: cts.Token);
             }
        }
    }
}

