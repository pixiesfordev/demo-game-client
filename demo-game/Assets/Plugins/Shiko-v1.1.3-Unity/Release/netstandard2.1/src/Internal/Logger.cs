using System;
using UnityEngine;

namespace Shiko.Internal.Logger
{
    internal static class Logger
    {
        public const string TRACE = "TRACE";
        public const string DEBUG = "DEBUG";
        public const string INFO = "INFO";
        public const string WARN = "WARN";
        public const string ERROR = "ERROR";
        public const string FATAL = "FATAL";

        public static void Trace(string message, params object[] args)
        {
            if (Mode.ModeManager.GetMode() == Mode.Mode.DEV)
            {
                Log(TRACE, message, args);
            }
        }

        public static void Debug(string message, params object[] args)
        {
            var mode = Mode.ModeManager.GetMode();
            if (mode == Mode.Mode.DEV || mode == Mode.Mode.TEST)
            {
                Log(DEBUG, message, args);
            }
        }

        public static void Info(string message, params object[] args)
        {
            Log(INFO, message, args);
        }

        public static void Warn(string message, params object[] args)
        {
            Log(WARN, message, args);
        }

        public static void Error(string message, params object[] args)
        {
            Log(ERROR, message, args);
        }

        public static void Fatal(string message, params object[] args)
        {
            Log(FATAL, message, args);
        }

        public static void SetMode(Mode.Mode mode)
        {
            Mode.ModeManager.SetMode(mode);
            Info("Log level set to: {0}", mode);
        }

        // Helper method to write logs
        private static void Log(string level, string message, params object[] args)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {string.Format(message, args)}");
        }
    }
}
