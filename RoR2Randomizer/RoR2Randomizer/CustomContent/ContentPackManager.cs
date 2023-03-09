using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using UnityEngine;

namespace RoR2Randomizer.CustomContent
{
    public class ContentPackManager : IContentPackProvider
    {
        readonly ContentPack _contentPack = new ContentPack();

        public string identifier => Main.PluginGUID;

        public static class Items
        {
            public static readonly ItemDef MonsterUseEquipmentDummyItem;

            static Items()
            {
                MonsterUseEquipmentDummyItem = ScriptableObject.CreateInstance<ItemDef>();
                MonsterUseEquipmentDummyItem.name = nameof(MonsterUseEquipmentDummyItem);

                // Setting the tier property here will not work because the ItemTierCatalog is not yet initialized
#pragma warning disable CS0618 // Type or member is obsolete
                MonsterUseEquipmentDummyItem.deprecatedTier = ItemTier.NoTier;
#pragma warning restore CS0618 // Type or member is obsolete

                MonsterUseEquipmentDummyItem.hidden = true;
                MonsterUseEquipmentDummyItem.canRemove = false;

                MonsterUseEquipmentDummyItem.AutoPopulateTokens();
            }
        }

        public void Init()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            _contentPack.identifier = identifier;

            _contentPack.entityStateTypes.Add(new Type[] { typeof(MultiEntityState) });

            _contentPack.itemDefs.Add(new ItemDef[]
            {
                Items.MonsterUseEquipmentDummyItem
            });

            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(_contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
