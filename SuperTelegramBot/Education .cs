using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;
using System.Data.SqlClient;

namespace PizzeriaBot
{
    class Education
    {
        static int count = 0;
        static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kirill\Desktop\SuperTelegramBot\SuperTelegramBot\Statistic.mdf;Integrated Security=True";
        static SqlConnection sql = new SqlConnection(connectionString);
        public static async void startEducation(Update update, ReplyKeyboardMarkup keyboard)

        {
            string[] EducationText = new string[] {
                "📊Статистика:\nЕсли нажмешь на кнопку статистика, то получишь текущую информацию о пиццерии.",
                "💵Деньги:\nИзначально ты должен Виктору большую денежную сумму, а именно 100000$. Работай и ты всего добьешься. Выполняя заказы ты получаешь деньги," +
                "которые можешь потрать на улучшение пиццерии.",
                "🍕Заказы:\nНажав на кнопку заказы тебе поступит новый заказ. Ты можешь приготовить заказ и получить деньги или отказаться от обслуживания, тогда этот клиент уйдет.",
                "⬆️Улучшение:\nУлучшение стоит некоторую сумму, которая при каждом повышении уровня пиццерии - увеличивается. Улучшение может привлечь новых клиентов, обновить меню или что-нибудь еще)."};
            var botClient = new TelegramBotClient("5045636554:AAHkHBI6LmECQQUo3rrcjmwBlj1Yg5YjQR0");
            var cts = new CancellationTokenSource();
            var chatId = update.Message.Chat.Id;
            if (count < EducationText.Length)
            {
                if (update.Message.Text == "Дальше➡️")
                {
                  await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: EducationText[count],
                    cancellationToken: cts.Token);
                  count++;
                }
            }else
            {
                count = 0;
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
                    text: "Итак ты попал на главное меню. С помощью него ты сможешь управлять своей пиццерией",
                    replyMarkup: keyboard,
                    cancellationToken: cts.Token);

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Смотри первый клиент!\nНажми на кнопку заказы",
                    cancellationToken: cts.Token);
            }
        }
    }
}


