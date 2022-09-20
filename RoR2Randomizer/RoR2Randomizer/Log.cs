using BepInEx.Logging;
using RoR2Randomizer.Utility;
using System.Diagnostics;

namespace RoR2Randomizer
{
    internal static class Log
    {
        internal static ManualLogSource _logSource;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        [Conditional("DEBUG")]
        internal static void Debug(string data)
        {
#if DEBUG
            _logSource.LogDebug(data);

            MiscUtils.SendChatMessage(data, "DEBUG");
#endif
        }

        internal static void Error(string data)
        {
            _logSource.LogError(data);

#if DEBUG
            MiscUtils.SendChatMessage(data, "ERROR");
#endif
        }

        internal static void Fatal(string data)
        {
            _logSource.LogFatal(data);

#if DEBUG
            MiscUtils.SendChatMessage(data, "FATAL");
#endif
        }

        internal static void Info(string data)
        {
            _logSource.LogInfo(data);

#if DEBUG
            MiscUtils.SendChatMessage(data, "INFO");
#endif
        }

        internal static void Message(string data)
        {
            _logSource.LogMessage(data);

#if DEBUG
            MiscUtils.SendChatMessage(data, "MESSAGE");
#endif
        }

        internal static void Warning(string data)
        {
            _logSource.LogWarning(data);

#if DEBUG
            MiscUtils.SendChatMessage(data, "WARNING");
#endif
        }
    }
}