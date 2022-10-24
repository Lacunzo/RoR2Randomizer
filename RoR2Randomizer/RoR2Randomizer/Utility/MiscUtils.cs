using BepInEx.Logging;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Utility
{
    public static class MiscUtils
    {
        public static void AddItem<T>(ref T[] array, T item)
        {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = item;
        }

        public static void AddItems<T>(ref T[] array, IEnumerable<T> items)
        {
            AddItems(ref array, items is T[] itemsArray ? itemsArray : items.ToArray());
        }

        public static void AddItems<T>(ref T[] array, params T[] items)
        {
            int itemsLength = items.Length;
            if (itemsLength > 0)
            {
                int oldLength = array.Length;
                Array.Resize(ref array, oldLength + itemsLength);
                Array.Copy(items, 0, array, oldLength, itemsLength);
            }
        }

#if DEBUG
        public static void TryNetworkLog(string message, LogLevel type)
        {
            if (Configuration.ConfigManager.Debug.AllowLocalhostConnect || NetworkServer.dontListen)
                return;

            LocalUser localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser != null)
            {
                NetworkUser networkUser = localUser.currentNetworkUser;
                if (networkUser)
                {
                    Networking.Debug.SyncConsoleLog syncConsoleLog = new Networking.Debug.SyncConsoleLog(message, type, networkUser.id);
                    if (NetworkServer.active)
                    {
                        syncConsoleLog.SendTo(NetworkDestination.Clients);
                    }
                    else if (NetworkClient.active)
                    {
                        syncConsoleLog.SendTo(NetworkDestination.Server);
                    }
                }
            }
        }
#endif

        public static bool TryAssign<T>(ref T dest, T value, IEqualityComparer<T> comparer = null)
        {
            if ((comparer ?? EqualityComparer<T>.Default).Equals(dest, value))
                return false;

            dest = value;
            return true;
        }

        public static LinkedList<T> CreateReverseLinkedListFromLinks<T>(T value, TryConvertToNextValue<T> tryConvertToNext)
        {
            LinkedList<T> result = new LinkedList<T>();

            do
            {
                result.AddFirst(value);
            } while (tryConvertToNext(ref value));

            return result;
        }

        public static LinkedList<T> CreateReverseLinkedListFromLinks<T>(T value, TryConvertDelegate<T> tryConvert)
        {
            return CreateReverseLinkedListFromLinks(value, tryConvert.ToConvertToNextValue());
        }

        public static LinkedList<T> CreateReverseLinkedListFromLinks<T>(T value, Func<T, T> tryGetNextValue) where T : class
        {
            return CreateReverseLinkedListFromLinks(value, tryGetNextValue.AddNullCheck());
        }

        public static void AppendDelegate<T>(ref T original, T additional) where T : Delegate
        {
            original = (T)Delegate.Combine(original, additional);
        }
    }
}
