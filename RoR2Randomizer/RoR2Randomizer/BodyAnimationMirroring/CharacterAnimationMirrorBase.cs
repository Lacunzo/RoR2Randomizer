using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoR2Randomizer.BodyAnimationMirroring
{
    public abstract class CharacterAnimationMirrorBase : MonoBehaviour
    {
        public Animator Animator { get; private set; }

        protected readonly List<BoneMarker> _boneMarkers = new List<BoneMarker>();

        protected IEnumerable<(BoneMarker originalBone, BoneMarker matchingBone)> findAllMatchingBones(CharacterAnimationMirrorBase other)
        {
            foreach (BoneMarker originalBone in _boneMarkers)
            {
                foreach (BoneMarker otherBone in other._boneMarkers)
                {
                    if (originalBone.Identifier == otherBone.Identifier)
                    {
#if DEBUG
                        Log.Debug($"found match {originalBone.name} -> {otherBone.name}");
#endif

                        yield return (originalBone, otherBone);
                    }
                }
            }
        }

        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
            if (Animator)
            {
                for (HumanBodyBones i = 0; i < HumanBodyBones.LastBone; i++)
                {
                    Transform boneTransform = Animator.GetBoneTransform(i);
                    if (boneTransform)
                    {
                        BoneMarker boneMarker = boneTransform.gameObject.AddComponent<BoneMarker>();
                        boneMarker.Identifier = i;

#if DEBUG
                        Log.Debug($"found bone with ID {i} ({boneTransform.name})");
#endif

                        _boneMarkers.Add(boneMarker);
                    }
                }
            }

            if (TryGetComponent(out CharacterModel characterModel))
            {
                setupBones(characterModel);
            }
        }

        void setupBones(CharacterModel characterModel)
        {
            if (characterModel.baseRendererInfos == null)
            {
                Log.Error($"{nameof(CharacterModel.baseRendererInfos)} is null");
                return;
            }

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in from ri in characterModel.baseRendererInfos
                                                                where ri.renderer && ri.renderer is SkinnedMeshRenderer
                                                                select (SkinnedMeshRenderer)ri.renderer)
            {
                foreach (Transform bone in skinnedMeshRenderer.bones)
                {
                    if (bone)
                    {
                        setupBone(bone);
                    }
                }
            }
        }

        void setupBone(Transform bone)
        {
            BoneMarker boneMarker = bone.gameObject.AddComponent<BoneMarker>();
            boneMarker.Identifier = bone.name;

            _boneMarkers.Add(boneMarker);
        }

        protected virtual void OnDestroy()
        {
            foreach (BoneMarker boneMarker in _boneMarkers)
            {
                if (boneMarker)
                {
                    Destroy(boneMarker);
                }
            }
        }
    }
}
