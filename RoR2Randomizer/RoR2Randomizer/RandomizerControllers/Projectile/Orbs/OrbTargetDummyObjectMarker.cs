using R2API;
using R2API.Networking;
using RoR2;
using RoR2Randomizer.Networking.DamageOrbTargetDummy;
using RoR2Randomizer.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile.Orbs
{
    public class OrbTargetDummyObjectMarker : NetworkBehaviour
    {
        // The threshold for when new objects should be requested
        const int MIN_ALLOWED_LOCAL_OBJECTS = 15;

        static bool _availableLocalInstancesRefreshScheduled = false;
        static readonly Stack<OrbTargetDummyObjectMarker> _availableLocalInstances = new Stack<OrbTargetDummyObjectMarker>();

        public static GameObject Prefab { get; private set; }

        internal static void InitNetworkPrefab()
        {
            const string PREFAB_NAME = "OrbTargetDummy";

            GameObject tmpPrefab = new GameObject(PREFAB_NAME + "_tmp");
            NetworkIdentity networkIdentity = tmpPrefab.AddComponent<NetworkIdentity>();
            networkIdentity.serverOnly = false;
            networkIdentity.localPlayerAuthority = true;

            NetworkTransform networkTransform = tmpPrefab.AddComponent<NetworkTransform>();
            networkTransform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
            networkTransform.syncRotationAxis = NetworkTransform.AxisSyncMode.None;
            networkTransform.syncSpin = false;
            networkTransform.interpolateMovement = 0f;

            tmpPrefab.AddComponent<OrbTargetDummyObjectMarker>();
            tmpPrefab.AddComponent<SetDontDestroyOnLoad>();
            Prefab = tmpPrefab.InstantiateClone(PREFAB_NAME);
            GameObject.Destroy(tmpPrefab);
        }

        [SystemInitializer]
        static void Init()
        {
            Run.onRunStartGlobal += onRunStart;

            ClientRequestOrbTargetMarkerObjects.Reply.OnReceive += ClientRequestDamageOrbTargetMarkerObjects_Reply_OnReceived;
        }

        static void onRunStart(Run _)
        {
            _availableLocalInstances.Clear();
            refillLocalInstances(MIN_ALLOWED_LOCAL_OBJECTS);
        }

        static void markLocalInstancesDirty()
        {
            if (_availableLocalInstancesRefreshScheduled)
                return;

            _availableLocalInstancesRefreshScheduled = true;
            RoR2Application.onNextUpdate += static () =>
            {
                OrbTargetDummyObjectMarker[] markers = _availableLocalInstances.ToArray();
                _availableLocalInstances.Clear();

                foreach (OrbTargetDummyObjectMarker marker in markers)
                {
                    if (!marker)
                        continue;

                    _availableLocalInstances.Push(marker);
                }

                _availableLocalInstancesRefreshScheduled = false;
            };
        }

        public static OrbTargetDummyObjectMarker GetMarker(Vector3 position, float? duration)
        {
            if (_availableLocalInstances.Count > 0)
            {
                OrbTargetDummyObjectMarker marker;
                do
                {
                    marker = _availableLocalInstances.Pop();
                } while (_availableLocalInstances.Count > 0 && (!marker || marker.isInUse));

                if (_availableLocalInstances.Count < MIN_ALLOWED_LOCAL_OBJECTS)
                {
                    refillLocalInstances((MIN_ALLOWED_LOCAL_OBJECTS - _availableLocalInstances.Count) * (!NetworkServer.active ? 3 : 1));
                }

                if (marker)
                {
                    marker.transform.position = position;
                    marker.setInUse(duration);

                    return marker;
                }
            }
            else
            {
                refillLocalInstances(MIN_ALLOWED_LOCAL_OBJECTS);
            }

            return null;
        }

        static void refillLocalInstances(int amount)
        {
#if DEBUG
            Log.Debug($"{nameof(amount)}={amount}");
#endif

            if (amount <= 0)
                return;

            if (NetworkServer.active)
            {
                for (int i = 0; i < amount; i++)
                {
                    OrbTargetDummyObjectMarker marker = InstantiateNew();
                    marker.IsAvailableToLocalPlayer = true;
                    NetworkServer.Spawn(marker.gameObject);
                    _availableLocalInstances.Push(marker);
                }
            }
            else if (NetworkClient.active)
            {
#if DEBUG
                Log.Debug($"asking server (client)");
#endif

                LocalUser localUser = LocalUserManager.GetFirstLocalUser();
                if (localUser != null)
                {
#if DEBUG
                    Log.Debug($"asking server ({nameof(localUser)})");
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
                            Log.Debug($"asking server ({nameof(networkUser)} + sending event) id: {networkUser.id.HasValidValue()} (waited {Time.time - timeStarted:F2} seconds)");
#endif

                            new ClientRequestOrbTargetMarkerObjects(amount, networkUser.id).SendTo(NetworkDestination.Server);
                        }

                        Main.Instance.StartCoroutine(waitForUserInitAndSendRequest((uint)amount, networkUser));
                    }
                }
            }
        }

        static void ClientRequestDamageOrbTargetMarkerObjects_Reply_OnReceived(OrbTargetDummyObjectMarker[] newTargetObjects)
        {
            foreach (OrbTargetDummyObjectMarker targetObj in newTargetObjects.Where(static o => o))
            {
#if DEBUG
                Log.Debug($"received orb marker: {targetObj.GetComponent<NetworkIdentity>().netId}");
#endif

                _availableLocalInstances.Push(targetObj);
            }
        }

        public static OrbTargetDummyObjectMarker InstantiateNew()
        {
            GameObject orbTarget = GameObject.Instantiate(Prefab);
            return orbTarget.GetComponent<OrbTargetDummyObjectMarker>();
        }

        public bool IsAvailableToLocalPlayer { get; internal set; }

        bool isAuthority => hasAuthority || (isServer && IsAvailableToLocalPlayer);

        bool _isInUse;
        public bool isInUse
        {
            get
            {
                return _isInUse;
            }
            set
            {
                if (MiscUtils.TryAssign(ref _isInUse, value))
                {
                    updateInUse();
                }
            }
        }

        float _timeToDisable;

        void Awake()
        {
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        void Run_onRunDestroyGlobal(Run obj)
        {
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            Run.onRunDestroyGlobal -= Run_onRunDestroyGlobal;

            if (_availableLocalInstances.Contains(this))
            {
                markLocalInstancesDirty();
            }
        }

        void setInUse(float? duration)
        {
            isInUse = true;

            if (duration.HasValue)
            {
                _timeToDisable = Time.time + duration.Value;
            }
            else
            {
                _timeToDisable = -1f;
            }
        }

        void updateInUse()
        {
            if (!isInUse)
            {
                if (isAuthority && !_availableLocalInstances.Contains(this))
                {
                    _availableLocalInstances.Push(this);
                }
            }
        }

        void Update()
        {
            if (!isAuthority)
                return;
            
            if (isInUse)
            {
                if (_timeToDisable > 0f && Time.time >= _timeToDisable)
                {
                    isInUse = false;
                }
            }
        }
    }
}
