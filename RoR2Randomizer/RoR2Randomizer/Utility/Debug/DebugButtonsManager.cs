#if DEBUG
using RoR2Randomizer.Patches.Debug;
using RoR2Randomizer.RandomizerControllers;
using UnityEngine;

namespace RoR2Randomizer.Utility.Debug
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
            else if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                InfiniteLunarCoin.ToggleEnabled();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                NoSkillCooldown.ToggleEnabled();
            }
        }
    }
}
#endif