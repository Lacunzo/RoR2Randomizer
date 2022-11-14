using R2API.Networking;
using RoR2Randomizer.Utility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.Networking.Generic
{
    public abstract class SyncGameObjectReference : NetworkMessageBase
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
            static IEnumerator baseTask(GameObject obj, CoroutineOut<NetworkInstanceId?> netId)
            {
                const string LOG_PREFIX = $"{nameof(SyncGameObjectReference)}.{nameof(waitForNetIdInitialized)} ";

#if DEBUG
                float timeStarted = Time.unscaledTime;
#endif

                while (obj && !obj.activeInHierarchy)
                {
                    yield return 0;
                }

                if (!obj)
                    yield break;

#if DEBUG
                Log.Debug(LOG_PREFIX + $"waited {Time.unscaledTime - timeStarted:F2} seconds for object {obj} enabled");
#endif

                NetworkIdentity netIdentity = null;
                while (obj && !(netIdentity = obj.GetComponent<NetworkIdentity>()))
                {
                    yield return 0;
                }

                if (!obj)
                    yield break;

                while (netIdentity && netIdentity.netId.IsEmpty())
                {
                    yield return 0;
                }

                if (!netIdentity)
                    yield break;

                netId.Result = netIdentity.netId;

#if DEBUG
                Log.Debug(LOG_PREFIX + $"waited {Time.unscaledTime - timeStarted:F2} seconds for object {obj} net init");
#endif
            }

            return baseTask(obj, netId).AddTimeout(OBJECT_WAIT_TIMEOUT);
        }

        static IEnumerator waitForNetIdThenSend(SyncGameObjectReference message, Action sendMessageFunc)
        {
            CoroutineOut<NetworkInstanceId?> netId = new CoroutineOut<NetworkInstanceId?>();
            yield return waitForNetIdInitialized(message._obj, netId);

            if (netId.Result.HasValue)
            {
                message._objectId = netId.Result.Value;
                sendMessageFunc();
            }
#if DEBUG
            else
            {
                Log.Warning($"Net Id for object {message._obj} could not be resolved, message will not be sent");
            }
#endif
        }

        public override void SendTo(NetworkDestination destination)
        {
            Main.Instance.StartCoroutine(waitForNetIdThenSend(this, () => base.SendTo(destination)));
        }

        public override void SendTo(NetworkConnection target)
        {
            Main.Instance.StartCoroutine(waitForNetIdThenSend(this, () => base.SendTo(target)));
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(_objectId);
        }

        public override void Deserialize(NetworkReader reader)
        {
            _objectId = reader.ReadNetworkId();
        }

        public static IEnumerator WaitForObjectResolved(NetworkInstanceId objectId, float? timeout, CoroutineOut<GameObject> resolvedObject)
        {
            static IEnumerator baseTask(NetworkInstanceId objectId, CoroutineOut<GameObject> resolvedObject)
            {
#if DEBUG
                float timeStarted = Time.unscaledTime;
#endif

                while (!(resolvedObject.Result = (NetworkServer.active ? NetworkServer.FindLocalObject(objectId)
                                                                       : ClientScene.FindLocalObject(objectId))))
                {
                    yield return 0;
                }
#if DEBUG
                Log.Debug($"{nameof(SyncGameObjectReference)} waited {Time.unscaledTime - timeStarted:F2} seconds for object ({resolvedObject.Result})");
#endif
            }

            IEnumerator task = baseTask(objectId, resolvedObject);
            if (timeout.HasValue)
            {
                task = task.AddTimeout(timeout.Value);
            }

            return task;
        }

        IEnumerator waitForObjectResolved()
        {
            CoroutineOut<GameObject> resolvedObject = new CoroutineOut<GameObject>();
            yield return WaitForObjectResolved(_objectId, OBJECT_WAIT_TIMEOUT, resolvedObject);

            if (resolvedObject.Result)
            {
                onReceivedObjectResolved(resolvedObject.Result);
            }
        }

        public override void OnReceived()
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
