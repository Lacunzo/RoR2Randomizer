using HG;
using R2API.Networking;
using RoR2;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.RandomizerControllers.SniperWeakPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.SniperWeakPointRandomizer
{
    public sealed class SyncSniperWeakPointReplacements : SyncGameObjectReference
    {
        readonly struct HurtBoxData
        {
            public readonly byte IndexPlusOne;
            public readonly bool OverrideIsSniperTarget;

            public HurtBoxData(HurtBox hurtBox) : this((byte)(hurtBox.indexInGroup + 1), hurtBox.isSniperTarget)
            {
            }

            HurtBoxData(byte indexPlusOne, bool overrideIsSniperTarget)
            {
                IndexPlusOne = indexPlusOne;
                OverrideIsSniperTarget = overrideIsSniperTarget;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(IndexPlusOne);
                writer.Write(OverrideIsSniperTarget);
            }

            public static HurtBoxData Deserialize(NetworkReader reader)
            {
                return new HurtBoxData(reader.ReadByte(), reader.ReadBoolean());
            }
        }

        int _totalLength;
        HurtBoxData[] _hurtBoxDatas;

        public SyncSniperWeakPointReplacements()
        {
        }

        public SyncSniperWeakPointReplacements(CharacterBody ownerBody, IEnumerable<HurtBox> hurtBoxes, int totalLength) : base(ownerBody.gameObject)
        {
            _totalLength = totalLength;
            _hurtBoxDatas = hurtBoxes.Where(h => h.indexInGroup >= 0).Select(h => new HurtBoxData(h)).ToArray();
        }

        public override void SendTo(NetworkDestination destination)
        {
            if (_hurtBoxDatas.Length > 0)
            {
                base.SendTo(destination);
            }
        }

        public override void SendTo(NetworkConnection target)
        {
            if (_hurtBoxDatas.Length > 0)
            {
                base.SendTo(target);
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WritePackedUInt32((uint)_totalLength);

            writer.WritePackedUInt32((uint)_hurtBoxDatas.Length);
            foreach (HurtBoxData hurtBox in _hurtBoxDatas)
            {
                hurtBox.Serialize(writer);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            _totalLength = (int)reader.ReadPackedUInt32();

            uint hurtBoxesLength = reader.ReadPackedUInt32();
            _hurtBoxDatas = new HurtBoxData[hurtBoxesLength];
            for (uint i = 0; i < hurtBoxesLength; i++)
            {
                _hurtBoxDatas[i] = HurtBoxData.Deserialize(reader);
            }
        }

        protected override bool shouldHandleEvent => !NetworkServer.active && NetworkClient.active;

        protected override void onReceivedObjectResolved(GameObject obj)
        {
#if DEBUG
            Log.Debug($"HurtBox root resolved ({obj})");
#endif

            if (!obj.TryGetComponent(out ModelLocator modelLocator))
                return;

            Transform modelTransform = modelLocator.modelTransform;
            if (!modelTransform || !modelTransform.TryGetComponent(out HurtBoxGroup hurtBoxGroup) || hurtBoxGroup.hurtBoxes == null)
                return;

            HurtBoxGroupRandomizerData hurtBoxGroupRandomizerData = hurtBoxGroup.gameObject.AddComponent<HurtBoxGroupRandomizerData>();
            hurtBoxGroupRandomizerData.Initialize(getOriginalIsSniperTargetValues(hurtBoxGroup), getOverrideIsSniperTargetValues());
        }

        bool[] getOriginalIsSniperTargetValues(HurtBoxGroup hurtBoxGroup)
        {
            bool[] originalIsSniperTargetValues = new bool[_totalLength];
            for (int i = 0; i < _totalLength; i++)
            {
                HurtBox hurtBox = ArrayUtils.GetSafe(hurtBoxGroup.hurtBoxes, i);
                if (hurtBox)
                {
                    originalIsSniperTargetValues[i] = hurtBox.isSniperTarget;
                }
            }

            return originalIsSniperTargetValues;
        }

        bool?[] getOverrideIsSniperTargetValues()
        {
            bool?[] overrideIsSniperTargetValues = new bool?[_totalLength];

            foreach (HurtBoxData hurtBoxData in _hurtBoxDatas)
            {
                int index = hurtBoxData.IndexPlusOne - 1;
                if (ArrayUtils.IsInBounds(overrideIsSniperTargetValues, index))
                {
                    overrideIsSniperTargetValues[index] = hurtBoxData.OverrideIsSniperTarget;
                }
            }

            return overrideIsSniperTargetValues;
        }
    }
}
