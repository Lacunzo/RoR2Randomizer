using System;
using UnityEngine;

namespace RoR2Randomizer.BodyAnimationMirroring
{
    public class BoneMarker : MonoBehaviour
    {
        public BoneIdentifier Identifier;

        SyncToTransform _transformSync;

        public bool HasForcedTargetTransform => _transformSync && _transformSync.Target;

        public void SetForceTargetBone(BoneMarker target)
        {
            if (!_transformSync)
            {
                _transformSync = gameObject.AddComponent<SyncToTransform>();
            }

            _transformSync.Target = target.transform;
        }

        public void ReturnToOriginalTransform(float tweenDuration)
        {
            if (HasForcedTargetTransform)
            {
                _transformSync.ResetToOriginalTransform(tweenDuration);
            }
        }

        void OnDestroy()
        {
            if (HasForcedTargetTransform)
            {
                Destroy(_transformSync);
            }
        }
    }
}
