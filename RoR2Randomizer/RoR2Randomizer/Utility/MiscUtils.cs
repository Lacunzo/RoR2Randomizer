﻿using BepInEx.Logging;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Networking.Debug;
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
            PlayerCharacterMasterController localPlayer = PlayerCharacterMasterController.instances.SingleOrDefault(p => p.isLocalPlayer);
            NetworkInstanceId localPlayerNetId = localPlayer ? localPlayer.NetworknetworkUserInstanceId : default;

            SyncConsoleLog syncConsoleLog = new SyncConsoleLog(message, type, localPlayerNetId);
            if (NetworkServer.active)
            {
                syncConsoleLog.Send(NetworkDestination.Clients);
            }
            else if (NetworkClient.active)
            {
                syncConsoleLog.Send(NetworkDestination.Server);
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
    }
}
