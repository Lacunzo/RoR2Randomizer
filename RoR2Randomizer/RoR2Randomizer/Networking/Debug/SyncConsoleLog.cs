using BepInEx.Logging;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.Debug
{
    public class SyncConsoleLog : INetMessage
    {
        LogLevel _logLevel;
        NetworkInstanceId _fromClient;
        string _log;

        public SyncConsoleLog()
        {
        }

        public SyncConsoleLog(string log, LogLevel logLevel, NetworkInstanceId fromClient)
        {
            _logLevel = logLevel;
            _fromClient = fromClient;
            _log = log;
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            writer.Write((int)_logLevel);
            writer.Write(_fromClient);
            writer.Write(_log);
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            _logLevel = (LogLevel)reader.ReadInt32();
            _fromClient = reader.ReadNetworkId();
            _log = reader.ReadString();
        }

        void INetMessage.OnReceived()
        {
#if DEBUG
            string fromClientName = null;

            PlayerCharacterMasterController controller = null;
            GameObject clientObject = Util.FindNetworkObject(_fromClient);
            if (clientObject && clientObject.TryGetComponent<PlayerCharacterMasterController>(out controller))
            {
                fromClientName = controller.GetDisplayName();
                if (string.IsNullOrEmpty(fromClientName))
                    fromClientName = null;
            }

            if (!controller || NetworkServer.active != controller.isServer)
            {
                fromClientName ??= "UNKNOWN";
                Log.LogType($"[{fromClientName}]: {_log}", _logLevel);
            }
#endif
        }
    }
}