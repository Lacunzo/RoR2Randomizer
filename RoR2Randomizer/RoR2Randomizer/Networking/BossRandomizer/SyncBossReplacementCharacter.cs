using R2API.Networking.Interfaces;
using RoR2Randomizer.RandomizerController.Boss;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.BossRandomizer
{
    public class SyncBossReplacementCharacter : INetMessage
    {
        GameObject _masterObject;
        BossReplacementType _replacementType;

        public SyncBossReplacementCharacter()
        {
        }

        public SyncBossReplacementCharacter(GameObject gameObject, BossReplacementType replacementType)
        {
            _masterObject = gameObject;
            _replacementType = replacementType;
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            writer.Write(_masterObject);
            writer.WritePackedUInt32((uint)_replacementType);
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            _masterObject = reader.ReadGameObject();
            _replacementType = (BossReplacementType)reader.ReadPackedUInt32();
        }

        void INetMessage.OnReceived()
        {
#if DEBUG
            Log.Debug($"{nameof(SyncBossReplacementCharacter)}.OnReceived(): _masterObject: {_masterObject}, _replacementType: {_replacementType}");
#endif
            if (!NetworkServer.active)
            {
#if DEBUG
                Log.Debug($"Recieved {nameof(SyncBossReplacementCharacter)} as non-server");
#endif

                switch (_replacementType)
                {
                    case BossReplacementType.MithrixNormal:
                    case BossReplacementType.MithrixHurt:
                    case BossReplacementType.MithrixPhase2:
#if DEBUG
                        Log.Debug($"Running {nameof(BossRandomizerController.Mithrix.HandleSpawnedMithrixCharacterClient)}");
#endif
                        BossRandomizerController.Mithrix.HandleSpawnedMithrixCharacterClient(_masterObject, _replacementType);
                        break;
                }
            }
#if DEBUG
            else
            {
                Log.Debug($"Recieved {nameof(SyncBossReplacementCharacter)} as server, skipping");
            }
#endif
        }
    }
}
