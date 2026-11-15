using System;
using BepInEx.Logging;

namespace ThreeWallJumps
{
    internal static class ModLog
    {
        private static ManualLogSource logger;
        private static bool events = true;
        private static LiveConsole console;
        private static string lastStatus;

        internal static void Initialize(ManualLogSource log, bool showConsole, bool logEvents)
        {
            logger = log;
            events = logEvents;
            if (!showConsole) return;
            try
            {
                console = LiveConsole.Open();
                Info("LIVE CONSOLE: green = accepted jump, cyan = status/reset, red = error.");
            }
            catch (Exception exception)
            {
                Error("Could not open the live console; logs will still go to BepInEx/LogOutput.log. " + exception);
            }
        }

        internal static void Info(string message)
        {
            logger?.LogInfo(message);
            Write("INFO", message, 11);
        }
        internal static void Event(string message)
        {
            if (!events) return;
            logger?.LogInfo(message);
            Write("LIVE", message, message.StartsWith("REGISTERED:") ? (ushort)10 : (ushort)11);
        }
        internal static void Error(string message)
        {
            logger?.LogError(message);
            Write("ERROR", message, 12);
        }
        private static void Write(string level, string message, ushort color)
        {
            try { console?.Write("[" + DateTime.Now.ToString("HH:mm:ss") + "] [" + level + "] " + message, color); }
            catch { }
        }
        internal static void SetStatus(string status)
        {
            if (lastStatus == status) return;
            lastStatus = status;
            try { console?.SetStatus(status); }
            catch { }
        }
        internal static void Close()
        {
            try { console?.Dispose(); }
            catch { }
            console = null;
        }
    }
}
