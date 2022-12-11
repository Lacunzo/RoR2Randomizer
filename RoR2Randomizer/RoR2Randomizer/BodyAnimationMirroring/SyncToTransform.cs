using System;
using UnityEngine;

namespace RoR2Randomizer.BodyAnimationMirroring
{
    public class SyncToTransform : MonoBehaviour
    {
        public Transform Target;

        new Transform transform;

        bool _isResetting;
        float _resetDuration;
        float _resetTimer;

        public void ResetToOriginalTransform(float tweenTime)
        {
            if (tweenTime <= 0f)
            {
                Destroy(this);
            }

            _isResetting = true;
            _resetDuration = tweenTime;
            _resetTimer = 0f;
        }

        void Awake()
        {
            transform = base.transform;
        }

        void LateUpdate()
        {
            if (!Target)
            {
                if (_isResetting)
                {
                    Destroy(this);
                }

                return;
            }

            if (_isResetting)
            {
                float resetFraction = _resetDuration / _resetTimer;
                transform.position = Vector3.Lerp(Target.position, transform.position, resetFraction);
                transform.rotation = Quaternion.Lerp(Target.rotation, transform.rotation, resetFraction);

                if (resetFraction >= 1f)
                {
                    Destroy(this);
                }
            }
            else
            {
                transform.position = Target.position;
                transform.rotation = Target.rotation;
            }
        }

        void Update()
        {
            if (_isResetting)
            {
                _resetTimer += Time.deltaTime;
            }
        }
    }
}
