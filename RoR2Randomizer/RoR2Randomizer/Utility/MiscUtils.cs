using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

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
            AddItems(ref array, items.ToArray());
        }

        public static void AddItems<T>(ref T[] array, params T[] items)
        {
            if (items.Length == 0)
                return;

            int oldLength = array.Length;
            Array.Resize(ref array, oldLength + items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                array[oldLength + i] = items[i];
            }
        }

        // Copied from R2API to prevent dependency issues
        public static void SendChatMessage(string message, string messageFrom)
        {
            Chat.SimpleChatMessage simpleChatMessage = new Chat.SimpleChatMessage
            {
                baseToken = "{0}: {1}",
                paramTokens = new string[2] { messageFrom, message }
            };

            Chat.SendBroadcastChat(simpleChatMessage);
        }

        public static bool TryAssign<T>(ref T dest, T value, IEqualityComparer<T> comparer = null)
        {
            if ((comparer ?? EqualityComparer<T>.Default).Equals(dest, value))
                return false;

            dest = value;
            return true;
        }
    }
}
