using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PizzeriaBot
{
    class Stade
    {
        static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kirill\Desktop\SuperTelegramBot\SuperTelegramBot\Statistic.mdf;Integrated Security=True";
        static SqlConnection sql = new SqlConnection(connectionString);
        public static int countRecords=8;


        private static SqlCommand command1;
        private static SqlCommand command2;
        private static SqlCommand command3;
        private static SqlCommand command4;
        private static SqlCommand command5;

        public static async void UpStadeAsync(Update update, int stade)
        {
            if (stade == 1)
            {
                command1 = new SqlCommand($"exec Добавление_стадии N'Пирог',100,N'Александр'", sql);
               
                command2 = new SqlCommand($"exec Добавление_стадии N'Мороженное',100,N'Викор'", sql);
                
                command3 = new SqlCommand($"exec Добавление_стадии N'Коктейль',100,N'Ирина'", sql);
               
                command4 = new SqlCommand($"exec Добавление_стадии N'Барбекю',400,N'Екатерина'", sql);
                
                command5 = new SqlCommand($"exec Добавление_стадии N'Салат Цезарь',200,N'Кирилл'", sql);
               
            }

            if (stade == 2)
            {
                command1 = new SqlCommand($"exec Добавление_стадии N'Пепперони х2',800,N'Александр'", sql);

                command2 = new SqlCommand($"exec Добавление_стадии N'Сок + Пирог',300,N'Викор'", sql);

                command3 = new SqlCommand($"exec Добавление_стадии N'Пепперони, Маргарита',800,N'Ирина'", sql);

                command4 = new SqlCommand($"exec Добавление_стадии N'Барбекю + Кола',500,N'Екатерина'", sql);

                command5 = new SqlCommand($"exec Добавление_стадии N'Пеппрони х2 + Неополитанская x2',2000,N'Антон'", sql);
            }
            if (stade <= 2)
            {
                sql.Open();
                    await command1.ExecuteNonQueryAsync();
                    await command2.ExecuteNonQueryAsync();
                    await command3.ExecuteNonQueryAsync();
                    await command4.ExecuteNonQueryAsync();
                    await command4.ExecuteNonQueryAsync();
                sql.Close();
            }
            if (stade == 3)
            {
                sql.Open();
                    SqlCommand commandSuper = new SqlCommand($"Update [dbo].[Order] Set Price = '{10000}'", sql);
                    await commandSuper.ExecuteNonQueryAsync();
                sql.Close();
            }
        }
    }
}
