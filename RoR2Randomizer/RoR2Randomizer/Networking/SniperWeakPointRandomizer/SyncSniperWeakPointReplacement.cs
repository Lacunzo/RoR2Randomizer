using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Networking.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.SniperWeakPointRandomizer
{
    public sealed class SyncSniperWeakPointReplacement : SyncGameObjectReference
    {
        byte _hurtBoxIndexPlusOne;

        bool _overrideIsSniperTarget;

        public SyncSniperWeakPointReplacement()
        {
        }

        public SyncSniperWeakPointReplacement(CharacterBody ownerBody, HurtBox hurtBox, bool? overrideIsSniperTarget = null) : base(ownerBody.gameObject)
        {
            _hurtBoxIndexPlusOne = (byte)(hurtBox.indexInGroup + 1);
            _overrideIsSniperTarget = overrideIsSniperTarget ?? hurtBox.isSniperTarget;
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(_hurtBoxIndexPlusOne);
            writer.Write(_overrideIsSniperTarget);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            _hurtBoxIndexPlusOne = reader.ReadByte();
            _overrideIsSniperTarget = reader.ReadBoolean();
        }

        protected override bool shouldHandleEvent => !NetworkServer.active && NetworkClient.active;

        protected override void onReceivedObjectResolved(GameObject obj)
        {
#if DEBUG
            Log.Debug($"{nameof(SyncSniperWeakPointReplacement)}: HurtBox root resolved ({obj})");
#endif

            HurtBoxReference hurtBoxRef = HurtBoxReference.FromRootObject(obj);
            hurtBoxRef.hurtBoxIndexPlusOne = _hurtBoxIndexPlusOne;

            HurtBox hurtBox = hurtBoxRef.ResolveHurtBox();
            
            static IEnumerator waitThenApplyIsSniperTarget(HurtBox hurtBox)
            {
                while (hurtBox && !hurtBox.gameObject.activeInHierarchy)
                {
                    yield return 0;
                }

                //for (int i = 0; i < 3; i++)
                //{
                //    yield return new WaitForFixedUpdate();
                //}

                if (!hurtBox)
                    yield break;

                if (hurtBox.isSniperTarget)
                {
                    if (!hurtBox.isInSniperTargetList)
                    {
                        HurtBox.sniperTargetsList.Add(hurtBox);
                        hurtBox.isInSniperTargetList = true;
                    }
                }
                else
                {
                    if (hurtBox.isInSniperTargetList)
                    {
                        HurtBox.sniperTargetsList.Remove(hurtBox);
                        hurtBox.isInSniperTargetList = false;
                    }
                }
            }

            hurtBox.isSniperTarget = _overrideIsSniperTarget;
            Main.Instance.StartCoroutine(waitThenApplyIsSniperTarget(hurtBox));
        }
    }
}
