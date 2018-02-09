using System;

namespace Kontur.ImageTransformer
{
    class Logger
    {
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
            Log(message, ConsoleColor.White);
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