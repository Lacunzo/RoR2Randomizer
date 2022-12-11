using RoR2;
using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.BodyAnimationMirroring
{
    public class CharacterAnimationMirrorOwner : CharacterAnimationMirrorBase
    {
        CharacterAnimationMirrorTarget _mirrorModel;
        public CharacterAnimationMirrorTarget MirrorModel
        {
            get
            {
                return _mirrorModel;
            }
            set
            {
                _mirrorModel = value;

                /*
                foreach (BoneMarker boneMarker in _boneMarkers)
                {
                    boneMarker.ReturnToOriginalTransform(0f);
                }
                */

                foreach (var boneMatch in findAllMatchingBones(value))
                {
                    boneMatch.originalBone.SetForceTargetBone(boneMatch.matchingBone);
                }
            }
        }

        public Animator MirrorAnimator
        {
            get
            {
                if (MirrorModel)
                    return MirrorModel.Animator;

                return null;
            }
        }

        public bool IsValid => !_isWaitingToDestroy;

        bool _isWaitingToDestroy;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (MirrorModel)
            {
                MirrorModel.CleanupSelf();
            }
        }

        public static Animator GetMirrorTargetAnimatorOrOriginal(Animator original)
        {
            if (original && original.TryGetComponent<CharacterAnimationMirrorOwner>(out CharacterAnimationMirrorOwner mirrorOwner) && mirrorOwner.IsValid)
            {
                Animator mirrorAnimator = mirrorOwner.MirrorAnimator;
                if (mirrorAnimator)
                {
                    return mirrorAnimator;
                }
            }

            return original;
        }

        public static CharacterAnimationMirrorOwner SetupForModelTransform(Transform modelTransform, BodyIndex mirrorBodyIndex)
        {
            CharacterAnimationMirrorOwner owner = modelTransform.gameObject.AddComponent<CharacterAnimationMirrorOwner>();
            owner.setupMirrorBody(mirrorBodyIndex);

            return owner;
        }

        void setupMirrorBody(BodyIndex mirrorBodyIndex)
        {
            if (mirrorBodyIndex == BodyIndex.None)
                return;

            CharacterBody mirrorBodyPrefab = BodyCatalog.GetBodyPrefabBodyComponent(mirrorBodyIndex);
            if (!mirrorBodyPrefab)
                return;

            // modelLocator property gets assigned in CharacterBody Awake, and since this is a prefab, we have to use GetComponent instead
            ModelLocator modelLocator = mirrorBodyPrefab.GetComponent<ModelLocator>();
            if (!modelLocator)
                return;

            Transform modelTransform = modelLocator.modelTransform;
            if (!modelTransform)
                return;

            Transform mirrorTargetTransform = Instantiate(modelTransform, transform.position, transform.rotation);
            CharacterAnimationMirrorTarget target = mirrorTargetTransform.gameObject.AddComponent<CharacterAnimationMirrorTarget>();
            target.Owner = this;
            MirrorModel = target;
        }

        public void StopMirroring(float tweenDuration)
        {
            if (tweenDuration <= 0f)
            {
                Destroy(this);
            }
            else
            {
                foreach (BoneMarker boneMarker in _boneMarkers)
                {
                    if (boneMarker)
                    {
                        if (boneMarker.HasForcedTargetTransform)
                        {
                            boneMarker.ReturnToOriginalTransform(tweenDuration);
                        }
                        else
                        {
                            Destroy(boneMarker);
                        }
                    }
                }

                StartCoroutine(waitThenDestroy(tweenDuration));
            }
        }

        IEnumerator waitThenDestroy(float waitTime)
        {
            _isWaitingToDestroy = true;

            yield return new WaitForSeconds(waitTime);

            Destroy(this);
        }
    }
}
