using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;

namespace PizzeriaBot
{
    class Start
    {
        public static async void startMess(Update update) // "встречающие" сообщения
        {
            var botClient = new TelegramBotClient("5045636554:AAHkHBI6LmECQQUo3rrcjmwBlj1Yg5YjQR0");
            var cts = new CancellationTokenSource();
            var chatId = update.Message.Chat.Id;

            String text = "Добро пожаловать. Меня зовут Виктор.\nЯ хочу предложить тебе сделку." +
                " Я могу дать тебе в долг столько денег, сколько схватило бы на покупку помещения и оборудования " +
                "для открытия новой пиццерии!\nНо с уловием что ты мне отдашь этот долг с процентами. После успешной сделки я позволю оставить пиццерию себе.";
            await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: text,
               cancellationToken: cts.Token);

            text = "Итак приступим. Я нашел подходящее место для нашего дела. Небольшое здание в центре города\nТебе я оставляю честь придумать название.";
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                cancellationToken: cts.Token);

            text = "Напиши название своей пиццерии в формате 'Пиццерия Example'. (Только приличное)";
           
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                cancellationToken: cts.Token,
                replyMarkup: new ForceReplyMarkup { Selective = true });
        }
        public static async void waitMessage (Update update, ReplyKeyboardMarkup keyboard) // ожидание ответа пользователя
        {
                var botClient = new TelegramBotClient("5045636554:AAHkHBI6LmECQQUo3rrcjmwBlj1Yg5YjQR0");
                var cts = new CancellationTokenSource();
                var chatId = update.Message.Chat.Id;
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: update.Message.Text.Remove(0,8).ToString() + "\nХорошее название.",
                    replyMarkup: keyboard,
                    cancellationToken: cts.Token);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Приступим к обучению",
                    replyMarkup: keyboard,
                    cancellationToken: cts.Token);
        }
     }
}

