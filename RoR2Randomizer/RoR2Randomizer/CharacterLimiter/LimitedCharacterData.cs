using RoR2;
using UnityEngine;

namespace RoR2Randomizer.CharacterLimiter
{
    public class LimitedCharacterData : MonoBehaviour
    {
        public static LimitedCharacterData AddGeneration(GameObject master, GameObject owner, LimitedCharacterType type)
        {
            LimitedCharacterData limitedCharacterData = master.AddComponent<LimitedCharacterData>();
            limitedCharacterData.Generation = GetGeneration(owner) + 1;
            limitedCharacterData.Type = type;
            return limitedCharacterData;
        }

        public static int GetGeneration(GameObject master)
        {
            return master && master.TryGetComponent<LimitedCharacterData>(out LimitedCharacterData data) ? data.Generation : 0;
        }

        public DeployableSlot DeployableType = DeployableSlot.None;
        public LimitedCharacterType Type;

        public int Generation;

        void Start()
        {
            if (TryGetComponent<Deployable>(out Deployable deployable) && deployable.ownerMaster)
            {
                foreach (DeployableInfo ownerDeployableInfo in deployable.ownerMaster.deployablesList)
                {
                    if (ownerDeployableInfo.deployable == deployable)
                    {
                        DeployableType = ownerDeployableInfo.slot;
                        break;
                    }
                }
            }
        }
    }
}
