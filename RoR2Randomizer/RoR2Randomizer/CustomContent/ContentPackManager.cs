using RoR2.ContentManagement;
using System;
using System.Collections;

namespace RoR2Randomizer.CustomContent
{
    public class ContentPackManager : IContentPackProvider
    {
        readonly ContentPack _contentPack = new ContentPack();

        public string identifier => Main.PluginGUID;

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
