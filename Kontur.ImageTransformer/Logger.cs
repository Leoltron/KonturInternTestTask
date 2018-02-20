using System;

namespace Kontur.ImageTransformer
{
    internal static class Logger
    {
        private static int id = 0;
        public static void Log(object message, ConsoleColor foreColor)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = foreColor;
            Console.WriteLine(message);
            Console.ForegroundColor = prevColor;
        }

        public static void Debug(object message)
        {
            Log(message, ConsoleColor.DarkGray);
        }

        public static void Info(object message)
        {
            var i = id++;
            Log($"[{i}] {message}", ConsoleColor.White);
        }

        public static void Warn(object message)
        {
            Log(message, ConsoleColor.Yellow);
        }

        public static void Error(object message)
        {
            Log(message, ConsoleColor.Red);
        }
    }
}