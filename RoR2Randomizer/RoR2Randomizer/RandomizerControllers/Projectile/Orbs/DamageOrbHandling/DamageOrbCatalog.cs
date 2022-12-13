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
            static void initIdentifiersForType<T>() where T : GenericDamageOrb, new()
            {
                static void initIdentifier(GenericDamageOrb damageOrb)
                {
                    DamageOrbIdentifier identifier = new DamageOrbIdentifier(damageOrb);
                    Instance.appendIdentifier(ref identifier, true);
                }

                initIdentifier(new T());
                initIdentifier(new T { isCrit = true });
            }

            initIdentifiersForType<HuntressArrowOrb>();
            initIdentifiersForType<HuntressFlurryArrowOrb>();
            initIdentifiersForType<LightningStrikeOrb>();
            initIdentifiersForType<MicroMissileOrb>();
            initIdentifiersForType<MissileVoidOrb>();
            initIdentifiersForType<SimpleLightningStrikeOrb>();
            initIdentifiersForType<SquidOrb>();
        }

        protected override DamageOrbIdentifier InvalidIdentifier => DamageOrbIdentifier.Invalid;

        protected override DamageOrbIdentifier createIdentifierForObject(GenericDamageOrb damageOrb)
        {
            return new DamageOrbIdentifier(damageOrb);
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
