using BepInEx.Logging;
using RoR2Randomizer.Utility;
using System.Diagnostics;
using UnityEngine;

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

            MiscUtils.TryNetworkLog(data, LogLevel.Debug);
#endif
        }

        internal static void Error(string data)
        {
            _logSource.LogError(data);

#if DEBUG
            MiscUtils.TryNetworkLog(data, LogLevel.Error);
#endif
        }

        internal static void Fatal(string data)
        {
            _logSource.LogFatal(data);

#if DEBUG
            MiscUtils.TryNetworkLog(data, LogLevel.Fatal);
#endif
        }

        internal static void Info(string data)
        {
            _logSource.LogInfo(data);

#if DEBUG
            MiscUtils.TryNetworkLog(data, LogLevel.Info);
#endif
        }

        internal static void Message(string data)
        {
            _logSource.LogMessage(data);

#if DEBUG
            MiscUtils.TryNetworkLog(data, LogLevel.Message);
#endif
        }

        internal static void Warning(string data)
        {
            _logSource.LogWarning(data);

#if DEBUG
            MiscUtils.TryNetworkLog(data, LogLevel.Warning);
#endif
        }

        internal static void LogType(string data, LogLevel level)
        {
#if !DEBUG
            if ((level & LogLevel.Debug) == 0)
#endif
            {
                _logSource.Log(level, data);
            }
        }
    }
}