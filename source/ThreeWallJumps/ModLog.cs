using BepInEx.Logging;

namespace ThreeWallJumps
{
    internal static class ModLog
    {
        private static ManualLogSource logger;
        private static bool events;

        internal static void Initialize(ManualLogSource log, bool logEvents)
        {
            logger = log;
            events = logEvents;
        }

        internal static void Info(string message)
        {
            logger?.LogInfo(message);
        }
        internal static void Event(string message)
        {
            if (!events) return;
            logger?.LogInfo(message);
        }
        internal static void Error(string message)
        {
            logger?.LogError(message);
        }
    }
}
