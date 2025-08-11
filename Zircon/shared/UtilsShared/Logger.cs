using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilsShared
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public static class Logger
    {
        private static readonly object _lock = new object();

        public static void Print(string message, LogLevel level = LogLevel.Info)
        {
            lock (_lock)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                switch (level)
                {
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;

                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;

                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    case LogLevel.Fatal:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                }

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}");

                Console.ForegroundColor = originalColor;
            }
        }
    }
}