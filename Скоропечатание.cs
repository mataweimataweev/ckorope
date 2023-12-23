using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace TypingTest
{
    public class User
    {
        public string Name { get; set; }
        public int CharactersPerMinute { get; set; }
        public int CharactersPerSecond { get; set; }
    }

    public static class Leaderboard
    {
        private const string LeaderboardFilePath = "leaderboard.json";
        private static List<User> _users;

        public static void Initialize()
        {
            if (File.Exists(LeaderboardFilePath))
            {
                var json = File.ReadAllText(LeaderboardFilePath);
                _users = JsonSerializer.Deserialize<List<User>>(json);
            }
            else
            {
                _users = new List<User>();
            }
        }

        public static void AddUser(User user)
        {
            _users.Add(user);
            SaveLeaderboard();
        }

        public static IEnumerable<User> GetTopUsers(int count)
        {
            return _users.OrderByDescending(u => u.CharactersPerMinute).Take(count);
        }

        private static void SaveLeaderboard()
        {
            var json = JsonSerializer.Serialize(_users);
            File.WriteAllText(LeaderboardFilePath, json);
        }
    }

    public class TypingTest
    {
        private static Stopwatch _timer;

        public static void Run()
        {
            Console.WriteLine("Введите свое имя:");
            var name = Console.ReadLine();

            _timer = new Stopwatch();
            _timer.Start();

            Console.WriteLine("Текст, который необходимо напечатать:");

            var textToType = "Dota 2 — многопользовательская командная компьютерная игра в жанре MOBA, разработанная и изданная корпорацией Valve. Игра является продолжением DotA — пользовательской карты-модификации для игры Warcraft III: Reign of Chaos и дополнения к ней Warcraft III: The Frozen Throne. Игра изображает сражение на карте особого вида; в каждом матче участвуют две команды по пять игроков, управляющих разными «героями» — персонажами с различными наборами способностей и характеристиками. Для победы в матче команда должна уничтожить особый объект — «крепость», принадлежащий вражеской стороне, и защитить от уничтожения собственную «крепость». Dota 2 работает по модели free-to-play с элементами микроплатежей";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(textToType);

            Console.ResetColor();

            var input = "";

            // Создаем и запускаем поток для чтения ввода от пользователя
            Thread inputThread = new Thread(() =>
            {
                while (true)
                {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        break;
                    }

                    input += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar);
                }
            });
            inputThread.Start();

            // Ожидаем завершения потока чтения ввода от пользователя
            inputThread.Join();

            _timer.Stop();
            var elapsedTime = _timer.ElapsedMilliseconds;
            var charactersPerMinute = (int)(60.0 / (elapsedTime / 1000.0) * input.Length);
            var charactersPerSecond = (int)(input.Length / (elapsedTime / 1000.0));

            var user = new User
            {
                Name = name,
                CharactersPerMinute = charactersPerMinute,
                CharactersPerSecond = charactersPerSecond
            };

            Leaderboard.AddUser(user);

            Console.WriteLine("\nТаблица рекордов:");
            var topUsers = Leaderboard.GetTopUsers(10);
            foreach (var topUser in topUsers)
            {
                Console.WriteLine($"{topUser.Name} - {topUser.CharactersPerMinute} cpm, {topUser.CharactersPerSecond} cps");
            }
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            Leaderboard.Initialize();

            while (true)
            {
                TypingTest.Run();

                Console.WriteLine("Хотите повторить тест? (y/n)");
                var choice = Console.ReadLine();
                if (!string.Equals(choice, "y", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
        }
    }
}
