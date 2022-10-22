using R2API.Networking;
using R2API.Networking.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.Networking.Generic
{
    public abstract class SyncGameObjectReference : INetMessage
    {
        const float OBJECT_WAIT_TIMEOUT = 2f;

        readonly GameObject _obj;
        NetworkInstanceId _objectId;

        public SyncGameObjectReference()
        {
        }

        protected SyncGameObjectReference(GameObject obj)
        {
            _obj = obj;
        }

        static IEnumerator waitForNetIdInitialized(GameObject obj, CoroutineOut<NetworkInstanceId?> netId)
        {
            const string LOG_PREFIX = $"{nameof(SyncGameObjectReference)}.{nameof(waitForNetIdInitialized)} ";

            float timeStarted = Time.unscaledTime;

            while (obj && !obj.activeInHierarchy)
            {
                yield return 0;

                if (Time.unscaledTime - timeStarted >= OBJECT_WAIT_TIMEOUT)
                    yield break;
            }

            if (!obj)
                yield break;

#if DEBUG
            Log.Debug(LOG_PREFIX + $"waited {Time.unscaledTime - timeStarted:F2} seconds for object enabled");
#endif

            NetworkIdentity netIdentity = null;
            while (obj && !(netIdentity = obj.GetComponent<NetworkIdentity>()))
            {
                yield return 0;

                if (Time.unscaledTime - timeStarted >= OBJECT_WAIT_TIMEOUT)
                    yield break;
            }

            if (!obj)
                yield break;

            while (netIdentity && netIdentity.netId.IsEmpty())
            {
                yield return 0;

                if (Time.unscaledTime - timeStarted >= OBJECT_WAIT_TIMEOUT)
                    yield break;
            }

            if (!netIdentity)
                yield break;

            netId.Result = netIdentity.netId;

#if DEBUG
            Log.Debug(LOG_PREFIX + $"waited {Time.unscaledTime - timeStarted:F2} seconds for object net init");
#endif
        }

        static IEnumerator waitForNetIdThenSend(SyncGameObjectReference message, Action<SyncGameObjectReference> sendMessageFunc)
        {
            CoroutineOut<NetworkInstanceId?> netId = new CoroutineOut<NetworkInstanceId?>();
            yield return waitForNetIdInitialized(message._obj, netId);

            if (netId.Result.HasValue)
            {
                message._objectId = netId.Result.Value;
                sendMessageFunc(message);
            }
#if DEBUG
            else
            {
                Log.Warning($"Net Id for object {message._obj} could not be resolved, message will not be sent");
            }
#endif
        }

        public void SendTo(NetworkDestination destination)
        {
            Main.Instance.StartCoroutine(waitForNetIdThenSend(this, m => m.Send(destination)));
        }

        public void SendTo(NetworkConnection target)
        {
            Main.Instance.StartCoroutine(waitForNetIdThenSend(this, m => m.Send(target)));
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
            float timeStarted = Time.unscaledTime;

            GameObject obj;
            while (!(obj = ClientScene.FindLocalObject(_objectId)))
            {
                yield return 0;

                if (Time.unscaledTime - timeStarted >= OBJECT_WAIT_TIMEOUT)
                {
#if DEBUG
                    Log.Warning($"{nameof(SyncGameObjectReference)} Timed out resolving object with ID {_objectId}");
#endif
                    yield break;
                }
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

            if (shouldHandleEvent)
            {
                if (_objectId.IsEmpty())
                {
                    Log.Warning($"{nameof(SyncGameObjectReference)} recieved empty object id, aborting");
                    return;
                }

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
