using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.DamageOrbTargetDummy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace RoR2Randomizer.RandomizerControllers.Projectile.DamageOrbHandling
{
    public class DamageOrbTargetDummyObjectMarker : NetworkBehaviour
    {
        // The threshold for when new objects should be requested
        const int LOCAL_OBJECTS_THRESHOLD = 20;

        static readonly List<DamageOrbTargetDummyObjectMarker> _availableLocalInstances = new List<DamageOrbTargetDummyObjectMarker>();

        public static GameObject Prefab { get; private set; }

        internal static void InitNetworkPrefab()
        {
            const string PREFAB_NAME = "DamageOrbTargetDummy";

            GameObject tmpPrefab = new GameObject(PREFAB_NAME + "_tmp");
            NetworkIdentity networkIdentity = tmpPrefab.AddComponent<NetworkIdentity>();
            networkIdentity.serverOnly = false;
            networkIdentity.localPlayerAuthority = true;

            NetworkTransform networkTransform = tmpPrefab.AddComponent<NetworkTransform>();
            networkTransform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
            networkTransform.syncRotationAxis = NetworkTransform.AxisSyncMode.None;
            networkTransform.syncSpin = false;
            networkTransform.interpolateMovement = 0f;

            Prefab = tmpPrefab.InstantiateClone(PREFAB_NAME);
            GameObject.Destroy(tmpPrefab);
            DamageOrbTargetDummyObjectMarker marker = Prefab.AddComponent<DamageOrbTargetDummyObjectMarker>();
            marker.enabled = false;
        }

        [SystemInitializer]
        static void Init()
        {
            Run.onRunStartGlobal += onRunStart;

            ClientRequestDamageOrbTargetMarkerObjects.Reply.OnReceive += ClientRequestDamageOrbTargetMarkerObjects_Reply_OnReceived;
        }

        static void onRunStart(Run _)
        {
            refillLocalInstances(LOCAL_OBJECTS_THRESHOLD - _availableLocalInstances.Count);
        }

        public static DamageOrbTargetDummyObjectMarker GetMarker(Vector3 position, float? duration)
        {
            if (_availableLocalInstances.Count < LOCAL_OBJECTS_THRESHOLD)
            {
                refillLocalInstances(LOCAL_OBJECTS_THRESHOLD / 2);
            }

            if (_availableLocalInstances.Count > 0)
            {
                DamageOrbTargetDummyObjectMarker marker = _availableLocalInstances.GetAndRemoveAt(0);
                marker.transform.position = position;
                marker.setInUse(duration);
                return marker;
            }

            return null;
        }

        static void refillLocalInstances(int amount)
        {
            const string LOG_PREFIX = $"{nameof(DamageOrbTargetDummyObjectMarker)}.{nameof(refillLocalInstances)} ";

#if DEBUG
            Log.Debug(LOG_PREFIX + $"{nameof(amount)}={amount}");
#endif

            if (amount <= 0)
                return;

            if (NetworkServer.active)
            {
                for (int i = 0; i < amount; i++)
                {
                    DamageOrbTargetDummyObjectMarker marker = InstantiateNew();
                    marker.IsAvailableToLocalPlayer = true;
                    NetworkServer.Spawn(marker.gameObject);
                    _availableLocalInstances.Add(marker);
                }
            }
            else if (NetworkClient.active)
            {
#if DEBUG
                Log.Debug(LOG_PREFIX + $"asking server (client)");
#endif

                LocalUser localUser = LocalUserManager.GetFirstLocalUser();
                if (localUser != null)
                {
#if DEBUG
                    Log.Debug(LOG_PREFIX + $"asking server ({nameof(localUser)})");
#endif

                    NetworkUser networkUser = localUser.currentNetworkUser;
                    if (networkUser)
                    {
                        static IEnumerator waitForUserInitAndSendRequest(uint amount, NetworkUser networkUser)
                        {
#if DEBUG
                            float timeStarted = Time.time;
#endif

                            while (networkUser && !networkUser.id.HasValidValue())
                            {
                                yield return 0;
                            }

                            if (!networkUser)
                                yield break;
#if DEBUG
                            Log.Debug(LOG_PREFIX + $"asking server ({nameof(networkUser)} + sending event) id: {networkUser.id.HasValidValue()} (waited {Time.time - timeStarted:F2} seconds)");
#endif

                            new ClientRequestDamageOrbTargetMarkerObjects(amount, networkUser.id).SendTo(NetworkDestination.Server);
                        }

                        Main.Instance.StartCoroutine(waitForUserInitAndSendRequest((uint)amount, networkUser));
                    }
                }
            }
        }

        static void ClientRequestDamageOrbTargetMarkerObjects_Reply_OnReceived(DamageOrbTargetDummyObjectMarker[] newTargetObjects)
        {
            IEnumerable<DamageOrbTargetDummyObjectMarker> validObjects = newTargetObjects.Where(static o => o);

#if DEBUG
            Log.Debug($"Received orb markers: [{string.Join(", ", validObjects.Select(static o => o.GetComponent<NetworkIdentity>().netId))}]");
#endif

            _availableLocalInstances.AddRange(validObjects);
        }

        public static DamageOrbTargetDummyObjectMarker InstantiateNew()
        {
            GameObject orbTarget = GameObject.Instantiate(Prefab);
            return orbTarget.GetComponent<DamageOrbTargetDummyObjectMarker>();
        }

        public bool IsAvailableToLocalPlayer { get; internal set; }

        bool _isInUse;
        float _timeToDestroy;

        void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
            Run.onRunDestroyGlobal += onRunEnd;
        }

        void onRunEnd(Run _)
        {
            if (hasAuthority)
            {
                _isInUse = false;
                enabled = false;
            }
        }

        public void setInUse(float? duration)
        {
            _isInUse = true;

            if (duration.HasValue)
            {
                _timeToDestroy = Time.time + duration.Value;
            }
            else
            {
                _timeToDestroy = -1f;
            }

            enabled = true;
            _availableLocalInstances.Remove(this);
        }

        void OnDisable()
        {
            _availableLocalInstances.Add(this);
        }

        void OnDestroy()
        {
            _availableLocalInstances.Remove(this);
            Run.onRunDestroyGlobal -= onRunEnd;
        }

        void Update()
        {
            if (!hasAuthority && (!isServer || !IsAvailableToLocalPlayer))
                return;
            
            if (_isInUse)
            {
                if (_timeToDestroy > 0f && Time.time >= _timeToDestroy)
                {
                    _isInUse = false;
                    enabled = false;
                }
            }
        }
    }
}
