using BepInEx.Logging;
using RoR2Randomizer.Utility;
using System.Runtime.CompilerServices;

namespace RoR2Randomizer
{
    internal static class Log
    {
        internal static ManualLogSource _logSource;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        static string getLogPrefix(string callerPath, string callerMemberName, int callerLineNumber)
        {
            const string MOD_NAME = "RoR2Randomizer";

            int modNameLastPathIndex = callerPath.LastIndexOf(MOD_NAME);
            if (modNameLastPathIndex >= 0)
            {
                callerPath = callerPath.Substring(modNameLastPathIndex + MOD_NAME.Length + 1);
            }

            return $"{callerPath}:{callerLineNumber} ({callerMemberName}) ";
        }

#if DEBUG
        internal static void Debug(string data, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1) => Debug_NoCallerPrefix(getLogPrefix(callerPath, callerMemberName, callerLineNumber) + data);
        internal static void Debug_NoCallerPrefix(string data)
        {
            _logSource.LogDebug(data);

            MiscUtils.TryNetworkLog(data, LogLevel.Debug);
        }
#endif

        internal static void Error(string data, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1) => Error_NoCallerPrefix(getLogPrefix(callerPath, callerMemberName, callerLineNumber) + data);
        internal static void Error_NoCallerPrefix(string data)
        {
            _logSource.LogError(data);

#if DEBUG
            MiscUtils.TryNetworkLog(data, LogLevel.Error);
#endif
        }

        internal static void Fatal(string data, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1) => Fatal_NoCallerPrefix(getLogPrefix(callerPath, callerMemberName, callerLineNumber) + data);
        internal static void Fatal_NoCallerPrefix(string data)
        {
            _logSource.LogFatal(data);

#if DEBUG
            MiscUtils.TryNetworkLog(data, LogLevel.Fatal);
#endif
        }

        internal static void Info(string data, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1) => Info_NoCallerPrefix(getLogPrefix(callerPath, callerMemberName, callerLineNumber) + data);
        internal static void Info_NoCallerPrefix(string data)
        {
            _logSource.LogInfo(data);

#if DEBUG
            MiscUtils.TryNetworkLog(data, LogLevel.Info);
#endif
        }

        internal static void Message(string data, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1) => Message_NoCallerPrefix(getLogPrefix(callerPath, callerMemberName, callerLineNumber) + data);
        internal static void Message_NoCallerPrefix(string data)
        {
            _logSource.LogMessage(data);

#if DEBUG
            MiscUtils.TryNetworkLog(data, LogLevel.Message);
#endif
        }

        internal static void Warning(string data, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = -1) => Warning_NoCallerPrefix(getLogPrefix(callerPath, callerMemberName, callerLineNumber) + data);
        internal static void Warning_NoCallerPrefix(string data)
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