#if DEBUG
using BepInEx.Logging;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Networking.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.Debug
{
    public class SyncConsoleLog : NetworkMessageBase
    {
        LogLevel _logLevel;
        NetworkUserId _fromClient;
        string _log;

        public SyncConsoleLog()
        {
        }

        public SyncConsoleLog(string log, LogLevel logLevel, NetworkUserId fromClient)
        {
            _logLevel = logLevel;
            _fromClient = fromClient;
            _log = log;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write((int)_logLevel);
            GeneratedNetworkCode._WriteNetworkUserId_None(writer, _fromClient);
            writer.Write(_log);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _logLevel = (LogLevel)reader.ReadInt32();
            _fromClient = GeneratedNetworkCode._ReadNetworkUserId_None(reader);
            _log = reader.ReadString();
        }

        public override void OnReceived()
        {
            if (_fromClient.HasValidValue())
            {
                NetworkUser fromUser = NetworkUser.readOnlyInstancesList.SingleOrDefault(user => user.id.Equals(_fromClient));
                if (fromUser && !fromUser.isLocalPlayer)
                {
                    Log.LogType($"[{fromUser.userName}]: {_log}", _logLevel);
                }
            }
        }
    }
}
#endif