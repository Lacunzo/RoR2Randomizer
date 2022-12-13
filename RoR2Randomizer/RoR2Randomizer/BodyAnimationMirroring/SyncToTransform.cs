using UnityEngine;

namespace RoR2Randomizer.BodyAnimationMirroring
{
    public class SyncToTransform : MonoBehaviour
    {
        public Transform Target;

        new Transform transform;

        Vector3 _originalLocalPosition;
        Quaternion _originalLocalRotation;

        bool _isAnimated;

        Vector3 _localPositionLastFrame;
        Quaternion _localRotationLastFrame;

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

            _localPositionLastFrame = _originalLocalPosition = transform.localPosition;
            _localRotationLastFrame = _originalLocalRotation = transform.localRotation;
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

            if (transform.localPosition != _localPositionLastFrame || transform.localRotation != _localRotationLastFrame)
            {
                _isAnimated = true;
            }
            else
            {
                _isAnimated = false;
            }

            if (_isResetting)
            {
                float resetFraction = _resetTimer / _resetDuration;

                Vector3 originalPosition;
                Quaternion originalRotation;
                if (_isAnimated)
                {
                    originalPosition = transform.position;
                    originalRotation = transform.rotation;
                }
                else
                {
                    originalPosition = transform.TransformPoint(_originalLocalPosition);

                    Transform parent = transform.parent;

                    Quaternion parentRotation = parent ? parent.rotation : Quaternion.identity;

                    originalRotation = parentRotation * _originalLocalRotation;
                }

                transform.position = Vector3.Lerp(Target.position, originalPosition, resetFraction);
                transform.rotation = Quaternion.Lerp(Target.rotation, originalRotation, resetFraction);

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

            _localPositionLastFrame = transform.localPosition;
            _localRotationLastFrame = transform.localRotation;
        }

        void OnDestroy()
        {
            if (transform)
            {
                transform.localPosition = _originalLocalPosition;
                transform.localRotation = _originalLocalRotation;
            }
        }
    }
}
