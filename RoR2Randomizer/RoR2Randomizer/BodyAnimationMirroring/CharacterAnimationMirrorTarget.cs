using RoR2;
using System;
using UnityEngine;

namespace RoR2Randomizer.BodyAnimationMirroring
{
    public class CharacterAnimationMirrorTarget : CharacterAnimationMirrorBase
    {
        public CharacterAnimationMirrorOwner Owner;

        new Transform transform;

        protected override void Awake()
        {
            base.Awake();

            transform = base.transform;

            if (Animator)
            {
                Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
            }

            HurtBoxGroup hurtBoxGroup = GetComponent<HurtBoxGroup>();
            if (hurtBoxGroup)
            {
                foreach (HurtBox hurtBox in hurtBoxGroup.hurtBoxes)
                {
                    GameObject.Destroy(hurtBox);
                }

                GameObject.Destroy(hurtBoxGroup);
            }
            else
            {
                foreach (HurtBox hurtBox in GetComponentsInChildren<HurtBox>(true))
                {
                    GameObject.Destroy(hurtBox);
                }
            }

            foreach (Collider collider in GetComponentsInChildren<Collider>(true))
            {
                GameObject.Destroy(collider);
            }

            CharacterModel characterModel = GetComponent<CharacterModel>();
            if (characterModel)
            {
                // Can't destroy it since other components require it
                characterModel.enabled = false;
            }
        }

        void Update()
        {
            if (!Owner || !transform)
                return;

            Transform cachedOwnerTransform = Owner.transform;

            transform.position = cachedOwnerTransform.position;
            transform.rotation = cachedOwnerTransform.rotation;
        }

        public void CleanupSelf()
        {
            Destroy(gameObject);
        }
    }
}
