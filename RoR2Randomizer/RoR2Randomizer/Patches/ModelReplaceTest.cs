#if DEBUG
using HG;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.Patches
{
    //[PatchClass]
    static class ModelReplaceTest
    {
        class ModelData
        {
            public BodyIndex BodyIndex;
            public CharacterModel CharacterModelPrefab;
        }

        static ModelData _currentModel;

        [SystemInitializer(typeof(BodyCatalog))]
        static void Init()
        {
            SetModel("HuntressBody");
        }

        static ModelData getModelData(BodyIndex bodyIndex)
        {
            ModelData result = new ModelData();
            result.BodyIndex = bodyIndex;
            result.CharacterModelPrefab = BodyCatalog.GetBodyPrefab(bodyIndex)?.GetComponent<ModelLocator>()?.modelTransform?.GetComponent<CharacterModel>();

            return result;
        }

        static void SetModel(string bodyName)
        {
            _currentModel = getModelData(BodyCatalog.FindBodyIndex(bodyName));
        }

        static void Apply()
        {
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
            On.RoR2.ModelSkinController.ApplySkin += ModelSkinController_ApplySkin;
        }

        static void Cleanup()
        {
            On.RoR2.CharacterModel.Awake -= CharacterModel_Awake;
            On.RoR2.ModelSkinController.ApplySkin -= ModelSkinController_ApplySkin;
        }

        static void ModelSkinController_ApplySkin(On.RoR2.ModelSkinController.orig_ApplySkin orig, ModelSkinController self, int skinIndex)
        {
        }

        static bool _disablePatch;

        static void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, RoR2.CharacterModel self)
        {
            orig(self);

            if (_disablePatch || _currentModel == null || !_currentModel.CharacterModelPrefab)
                return;

            //ModelData originalModelData = getModelData(self.body.bodyIndex);

            Dictionary<string, Transform> boneByName = new Dictionary<string, Transform>();

            foreach (CharacterModel.RendererInfo renderInfo in self.baseRendererInfos)
            {
                if (renderInfo.renderer)
                {
                    if (renderInfo.renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                    {
                        foreach (Transform bone in skinnedMeshRenderer.bones)
                        {
                            if (!bone)
                                continue;

                            string name = bone.name;
                            if (!boneByName.ContainsKey(name))
                            {
                                boneByName.Add(name, bone);
                            }
                            else if (boneByName[name] != bone)
                            {
                                Log.Warning($"Duplicate bone name {name}");
                            }
                        }
                    }

                    renderInfo.renderer.gameObject.SetActive(false);
                }
            }

            foreach (Animator animator in self.GetComponentsInChildren<Animator>())
            {
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            _disablePatch = true;
            CharacterModel newModel = GameObject.Instantiate(_currentModel.CharacterModelPrefab, self.transform);
            _disablePatch = false;
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localRotation = Quaternion.identity;
            newModel.transform.localScale /= self.transform.localScale.ComponentMax();

            foreach (Animator animator in newModel.GetComponentsInChildren<Animator>())
            {
                animator.enabled = false;
            }

            foreach (CharacterModel.RendererInfo renderInfo in newModel.baseRendererInfos)
            {
                if (renderInfo.renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    foreach (Transform newModelBone in skinnedMeshRenderer.bones)
                    {
                        if (newModelBone && !newModelBone.GetComponent<SyncBoneTransform>())
                        {
                            if (boneByName.TryGetValue(newModelBone.name, out Transform baseModelBone))
                            {
                                SyncBoneTransform syncBoneTransform = newModelBone.gameObject.AddComponent<SyncBoneTransform>();
                                syncBoneTransform.Target = baseModelBone;
                            }
                        }
                    }
                }
            }

            HurtBoxGroup hurtBoxGroup = newModel.GetComponent<HurtBoxGroup>();
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
                foreach (HurtBox hurtBox in newModel.GetComponentsInChildren<HurtBox>(true))
                {
                    GameObject.Destroy(hurtBox);
                }
            }

            foreach (Collider collider in newModel.GetComponentsInChildren<Collider>(true))
            {
                GameObject.Destroy(collider);
            }
        }

        class SyncBoneTransform : MonoBehaviour
        {
            public Transform Target;

            public new Transform transform { get; private set; }

            void Awake()
            {
                transform = base.transform;
            }

            void LateUpdate()
            {
                if (!Target)
                    return;

                transform.position = Target.position;
                transform.rotation = Target.rotation;
            }
        }
    }
}
#endif