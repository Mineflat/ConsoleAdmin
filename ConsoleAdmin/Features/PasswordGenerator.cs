using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using Spectre.Console;
using PasswordGenerator;

namespace ConsoleAdmin.Features
{
    internal static class PasswordGenerator
    {
        private static Random r = new Random();
        /// <summary>
        /// Генерация случайных секретов в формате XXXX-XXXX-XXXX-XXXX, где X - случайный символ (буква или цифра).
        /// </summary>
        /// <param name="secretsCount"></param>
        /// <returns></returns>
        public static List<string> GenerateSecrets(int secretsCount = 1)
        {
            List<string> secrets = new List<string>();
            var faker = new Bogus.Randomizer();
            var rng = new Random();
            string secret;
            List<string> secret_parts;
            for (int i = 0; i < secretsCount; i++)
            {
                secret = string.Empty;
                secret_parts = new List<string>();
                for (int j = 0; j < r.Next(4, 5); j++)
                {
                    string part = faker.AlphaNumeric(5);
                    var randomized = new StringBuilder();
                    foreach (char c in part)
                    {
                        if (char.IsLetter(c))
                            randomized.Append(rng.Next(2) == 0 ? char.ToUpper(c) : char.ToLower(c));
                        else
                            randomized.Append(c);
                    }
                    secret_parts.Add(randomized.ToString());
                }
                secret = string.Join("-", secret_parts);
                secrets.Add(secret.Trim());
            }
            return secrets;
        }
        /// <summary>
        /// Отображение сгенерированных секретов в виде таблицы с использованием Spectre.Console.
        /// </summary>
        /// <param name="secretsCount"></param>
        public static void RenderSecrets(int secretsCount = 0)
        {
            if (secretsCount <= 0)
            {
                //secretsCount = Console.WindowHeight - 5; // Количество сгенерированных секретов по умолчанию (вся консоль, минус 5 строк для заголовка и нижнего колонтитула)
                secretsCount = Console.WindowHeight - 7; // Количество сгенерированных секретов по умолчанию (вся консоль, минус 5 строк для заголовка, хедера таблицы и нижнего колонтитула)
            }

            // Создание таблицы для заполнение ее сгенерированными секретами
            Table secretsTable = new Table()
            {
                Border = TableBorder.Rounded,
                BorderStyle = Style.Parse("deepskyblue1"),
                Expand = true,
                //ShowHeaders = false
            };
            secretsTable.AddColumn(new TableColumn("Номер").NoWrap().Centered());
            secretsTable.AddColumn(new TableColumn("Пароль").NoWrap().Centered());
            secretsTable.AddColumn(new TableColumn("Энтропия").NoWrap().Centered());



            string lasppass = string.Empty;
            string entropySCore = string.Empty;
            int entropy = 0;

            List<string> b = GenerateSecrets(secretsCount);
            List<string> sorted = b
                .OrderByDescending(s => ShannonEntropy(s))
                .ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                lasppass = sorted[i];
                entropy = (int)ShannonEntropy(lasppass);
                switch (entropy)
                {
                    case < 1:
                        entropySCore = $"~ [deeppink2]{entropy}[/][white]/5[/][deeppink2]  ([underline]Плохо![/])[/]"; // Секрет с низкой энтропией
                        break;
                    case >= 4:
                        entropySCore = $"~ [chartreuse1]{entropy}[/][white]/5[/][chartreuse1] ([bold]Отлично![/])[/]"; // Секрет с высокой энтропией
                        break;
                    default:
                        entropySCore = $"~ [yellow3_1]{entropy}[/][white]/5[/][yellow3_1] (Нормально)[/]"; // Секрет с средней энтропией
                        break;
                }
                secretsTable.AddRow($"[deepskyblue2]{i + 1}[/]", $"[white]{lasppass}[/]", $"{entropySCore}");
                lasppass = string.Empty; // Сброс пароля, чтоб не валялся в памяти после цикла
            }
            
            // Генерация лайоута с использованием Spectre.Console
            Layout root = new Layout("Secrets")
                .SplitColumns(
                        new Layout("Left") { Ratio = 2 },
                        new Layout("Center") { Ratio = 3 }
                            .SplitRows(
                                new Layout("Top") { Size = 1 },
                                new Layout("Middle") { Ratio = 1 },
                                new Layout("Timestamp") { Size = 1 },
                                new Layout("Bottom") { Size = 1 }
                            ),
                        new Layout("Right") { Ratio = 2 }
                );
            root["Secrets"]["Center"]["Top"].Update(new Rule("[lime]Генератор паролей[/]") { Justification = Justify.Center, Style = Style.Parse("deepskyblue1 dim") });
            root["Secrets"]["Center"]["Middle"].Update(secretsTable.Expand());
            root["Secrets"]["Center"]["Timestamp"].Update(new Rule($"[white]Обновлено [lime]{DateTime.Now.ToUniversalTime()}[/][/]") { Style = Style.Parse("deepskyblue1 dim") });
            root["Secrets"]["Center"]["Bottom"].Update(new Rule("[grey]Escape или Q - выйти. Любая другая клавиша немедленно перегенерирует все пароли[/]") { Style = Style.Parse("deepskyblue1 dim") });

            root["Secrets"]["Left"].Update(new Panel("").Border(BoxBorder.None).Expand());
            root["Secrets"]["Right"].Update(new Panel("").Border(BoxBorder.None).Expand());
            // Отображение сгенерированного лайоута
            AnsiConsole.Write(root);
        }

        public static void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.CursorVisible = false;
                RenderSecrets();
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                    return;
            }
        }

        /// <summary>
        /// returns bits of entropy represented in a given string, per 
        /// http://en.wikipedia.org/wiki/Entropy_(information_theory) 
        /// </summary>
        public static double ShannonEntropy(string s)
        {
            var map = new Dictionary<char, int>();
            foreach (char c in s)
            {
                if (!map.ContainsKey(c))
                    map.Add(c, 1);
                else
                    map[c] += 1;
            }

            double result = 0.0;
            int len = s.Length;
            foreach (var item in map)
            {
                var frequency = (double)item.Value / len;
                result -= frequency * (Math.Log(frequency) / Math.Log(2));
            }

            return result;
        }
    }
}
