using RoR2;
using RoR2Randomizer.Networking.ExplicitSpawnRandomizer;
using RoR2Randomizer.Utility;
using System;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.ExplicitSpawn
{
    static class FullExplicitSpawnInitListener
    {
        static readonly RunSpecific<bool> _explicitSpawnRandomizerEnabledAvailable = new RunSpecific<bool>(static () => NetworkServer.active && ExplicitSpawnRandomizerController.IsActive);
        static readonly RunSpecific<bool> _characterReplacementsInitialized = new RunSpecific<bool>();

        static readonly RunSpecific<bool> _hasDispatchedInitForCurrentRun = new RunSpecific<bool>();

        public static event Action OnFullInit;

        [SystemInitializer]
        static void Init()
        {
            SyncExplicitSpawnRandomizerEnabled.OnReceive += SyncExplicitSpawnRandomizerEnabled_OnReceive;
            CharacterReplacements.OnCharacterReplacementsInitialized += characterReplacementsInitialized;

            RoR2Application.onUpdate += update;
        }

        static void SyncExplicitSpawnRandomizerEnabled_OnReceive(bool isEnabled, bool randomizeHeretic)
        {
            _explicitSpawnRandomizerEnabledAvailable.Value = true;
        }

        static void characterReplacementsInitialized()
        {
            _characterReplacementsInitialized.Value = true;
        }

        static void update()
        {
            if (Run.instance && !_hasDispatchedInitForCurrentRun)
            {
                if (_explicitSpawnRandomizerEnabledAvailable && _characterReplacementsInitialized)
                {
                    OnFullInit?.Invoke();
                    _hasDispatchedInitForCurrentRun.Value = true;
                }
            }
        }
    }
}
