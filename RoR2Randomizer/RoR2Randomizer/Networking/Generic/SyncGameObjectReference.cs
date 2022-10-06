using R2API.Networking.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.Generic
{
    public abstract class SyncGameObjectReference : INetMessage
    {
        NetworkInstanceId _objectId;

        public SyncGameObjectReference()
        {
        }

        protected SyncGameObjectReference(GameObject obj)
        {
            _objectId = obj.GetComponent<NetworkIdentity>().netId;

            if (_objectId.IsEmpty())
            {
                Log.Warning($"{nameof(SyncGameObjectReference)} is about to sync an empty netId");
            }
        }

        public virtual void Serialize(NetworkWriter writer)
        {
            writer.Write(_objectId);
        }

        public virtual void Deserialize(NetworkReader reader)
        {
            _objectId = reader.ReadNetworkId();
        }

        IEnumerator waitForObjectResolved()
        {
#if DEBUG
            float timeStarted = Time.unscaledTime;
#endif

            GameObject obj;
            while (!(obj = ClientScene.FindLocalObject(_objectId)))
            {
                yield return 0;
            }

#if DEBUG
            Log.Debug($"{nameof(SyncGameObjectReference)} waited {Time.unscaledTime - timeStarted:F2} seconds for object");
#endif

            onReceivedObjectResolved(obj);
        }

        void INetMessage.OnReceived()
        {
#if DEBUG
            Log.Debug($"{nameof(SyncGameObjectReference)} received");
#endif

            if (_objectId.IsEmpty())
            {
                Log.Warning($"{nameof(SyncGameObjectReference)} recieved empty object id, aborting");
                return;
            }

            if (shouldHandleEvent)
            {
#if DEBUG
                Log.Debug($"{nameof(SyncGameObjectReference)} handling event");
#endif

                Main.Instance.StartCoroutine(waitForObjectResolved());
            }
#if DEBUG
            else
            {
                Log.Debug($"{nameof(SyncGameObjectReference)} should not handle event");
            }
#endif
        }

        protected virtual bool shouldHandleEvent => true;

        protected abstract void onReceivedObjectResolved(GameObject obj);
    }
}
