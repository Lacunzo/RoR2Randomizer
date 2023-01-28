using HG;
using R2API.Networking;
using RoR2;
using RoR2.Orbs;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.ProjectileRandomizer.Orbs.GenericDamage;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile.Orbs.DamageOrbHandling
{
    public class DamageOrbCatalog : GenericNetworkedCatalog<GenericDamageOrb, DamageOrbIdentifier>
    {
        public static readonly DamageOrbCatalog Instance = new DamageOrbCatalog();

        static DamageOrbCatalog()
        {
            SyncDamageOrbCatalog.OnReceive += Instance.SyncCatalog_OnReceive;
            SyncDamageOrbIndexNeeded.OnReceive += Instance.SyncIndexNeeded_OnReceive;
        }

        [SystemInitializer(typeof(EffectCatalog))]
        static void Init()
        {
            static void initIdentifier(GenericDamageOrb damageOrb)
            {
                DamageOrbIdentifier identifier = new DamageOrbIdentifier(damageOrb);
                Instance.appendIdentifier(ref identifier, true);
            }

            static void initIdentifiersForType<T>() where T : GenericDamageOrb, new()
            {
                initIdentifier(new T());
                initIdentifier(new T { isCrit = true });
            }

            static void initIdentifiersForType_Arg<T>(params object[] constructorParams) where T : GenericDamageOrb
            {
                T nonCritInstance = (T)Activator.CreateInstance(typeof(T), constructorParams);
                initIdentifier(nonCritInstance);

                T critInstance = (T)Activator.CreateInstance(typeof(T), constructorParams);
                critInstance.isCrit = true;
                initIdentifier(critInstance);
            }

            initIdentifiersForType<HuntressArrowOrb>();
            initIdentifiersForType<HuntressFlurryArrowOrb>();
            initIdentifiersForType<LightningStrikeOrb>();
            initIdentifiersForType<MicroMissileOrb>();
            initIdentifiersForType<MissileVoidOrb>();
            initIdentifiersForType<SimpleLightningStrikeOrb>();
            initIdentifiersForType<SquidOrb>();

            GameObject chainGunOrbEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/DroneWeapons/ChainGunOrbEffect.prefab").WaitForCompletion();
            initIdentifiersForType_Arg<ChainGunOrb>(chainGunOrbEffectPrefab);
        }

        protected override DamageOrbIdentifier InvalidIdentifier => DamageOrbIdentifier.Invalid;

        protected override bool tryCreateIdentifierForObject(GenericDamageOrb damageOrb, out DamageOrbIdentifier identifier)
        {
            if (damageOrb != null)
            {
                Type damageOrbType = damageOrb.GetType();
                if (damageOrbType == typeof(ChainGunOrb) || damageOrbType.GetConstructor(Array.Empty<Type>()) != null)
                {
                    identifier = new DamageOrbIdentifier(damageOrb);
                    return true;
                }
#if DEBUG
                else
                {
                    Log.Debug($"Not including Orb {damageOrbType.FullName} due to: no valid constructor");
                }
#endif
            }

            identifier = DamageOrbIdentifier.Invalid;
            return false;
        }

        protected override NetworkMessageBase getSyncIdentifierNeededMessage(in DamageOrbIdentifier identifier)
        {
            return new SyncDamageOrbIndexNeeded(identifier);
        }

        public override IEnumerable<NetworkMessageBase> GetNetMessages()
        {
            yield return new SyncDamageOrbCatalog(_identifiers, _identifiersCount);
        }

        public IEnumerable<ProjectileTypeIdentifier> GetAllDamageOrbProjectileIdentifiers()
        {
            return this.Select<DamageOrbIdentifier, ProjectileTypeIdentifier>(i => i);
        }
    }
}
