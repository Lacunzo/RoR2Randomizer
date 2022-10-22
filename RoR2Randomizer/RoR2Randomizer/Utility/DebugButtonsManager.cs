#if DEBUG
using RoR2;
using RoR2Randomizer.Patches.Debug;
using RoR2Randomizer.RandomizerControllers;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Utility
{
    [RandomizerController]
    public class DebugButtonsManager : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                SpawnDisabler.ToggleSpawnsDisabled();
            }
        }
    }
}
#endif