using HG;
using R2API.Networking;
using RoR2;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.ProjectileRandomizer.Bullet;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling
{
    // Not identified by this method:
    // EntityStates.TitanMonster.FireMegaLaser

    public class BulletAttackCatalog : GenericNetworkedCatalog<BulletAttack, BulletAttackIdentifier>
    {
        public static readonly BulletAttackCatalog Instance = new BulletAttackCatalog();

        BulletAttackCatalog() : base()
        {
            SyncBulletAttackCatalog.OnReceive += SyncCatalog_OnReceive;
            SyncBulletAttackIndexNeeded.OnReceive += SyncIndexNeeded_OnReceive;
        }

        [SystemInitializer(typeof(EffectCatalog))]
        static void Init()
        {
            #region Predefined Identifiers
            static void initIdentifier(string tracerEffectName, string hitEffectName, DamageType damageType = DamageType.Generic, BulletAttackFlags flags = BulletAttackFlags.None)
            {
                static EffectIndex findEffectIndex(string name)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        for (int i = 0; i < EffectCatalog.effectCount; i++)
                        {
                            EffectDef effectDef = EffectCatalog.GetEffectDef((EffectIndex)i);
                            if (effectDef != null)
                            {
                                if (effectDef.prefabName == name)
                                {
                                    return effectDef.index;
                                }
                            }
                        }
                    }

                    return EffectIndex.Invalid;
                }

                BulletAttackIdentifier identifier = new BulletAttackIdentifier(findEffectIndex(tracerEffectName), findEffectIndex(hitEffectName), damageType, flags);

#if DEBUG
                const bool IS_DEBUG = true;
#else
                const bool IS_DEBUG = false;
#endif

                Instance.appendIdentifier(ref identifier, IS_DEBUG);
            }

            // EntityStates.Bandit2.Weapon.FireSidearmResetRevolver
            initIdentifier("TracerBanditPistol", "HitsparkBandit2Pistol", DamageType.BonusToLowHealth | DamageType.ResetCooldownsOnKill);

            // EntityStates.Bandit2.Weapon.FireSidearmSkullRevolver
            initIdentifier("TracerBanditPistol", "HitsparkBandit2Pistol", DamageType.BonusToLowHealth | DamageType.GiveSkullOnKill);

            // EntityStates.CaptainSupplyDrop.HitGroundState
            initIdentifier(null, "OmniImpactVFXSlash");

            // EntityStates.ClayBruiser.Weapon.MinigunFire
            initIdentifier("TracerClayBruiserMinigun", "Hitspark1");

            // EntityStates.Commando.CommandoWeapon.FireBarrage
            initIdentifier("TracerCommandoBoost", "HitsparkCommandoBarrage", DamageType.Stun1s);
            
            // EntityStates.Commando.CommandoWeapon.FireLightsOut
            initIdentifier("TracerBanditPistol", "HitsparkBanditPistol", DamageType.ResetCooldownsOnKill);
            
            // EntityStates.Commando.CommandoWeapon.FirePistol2
            initIdentifier("TracerCommandoDefault", "HitsparkCommando");
            
            // EntityStates.Commando.CommandoWeapon.FireShotgun
            initIdentifier("TracerBanditShotgun", "HitsparkBandit");

            // EntityStates.Commando.CommandoWeapon.FireShrapnel
            initIdentifier("TracerBanditPistol", "WeakPointProcEffect");

            // EntityStates.Drone.DroneWeapon.FireGatling
            // EntityStates.Drone.DroneWeapon.FireTurret
            initIdentifier("TracerNoSmoke", "Hitspark1");

            // EntityStates.Drone.DroneWeapon.FireMegaTurret
            initIdentifier("TracerCommandoBoost", "MuzzleflashBarrage");

            // EntityStates.EngiTurret.EngiTurretWeapon.FireBeam
            initIdentifier(null, "Hitspark1", DamageType.SlowOnHit);

            // EntityStates.EngiTurret.EngiTurretWeapon.FireGauss
            initIdentifier("TracerEngiTurret", "ImpactEngiTurret");

            // EntityStates.Bandit2.Weapon.Bandit2FireRifle
            initIdentifier("TracerBandit2Rifle", "HitsparkBandit");

            // EntityStates.Bandit2.Weapon.FireShotgun2
            initIdentifier("TracerBandit2Shotgun", "HitsparkBandit");

            // EntityStates.Captain.Weapon.FireCaptainShotgun
            initIdentifier("TracerCaptainShotgun", "HitsparkCaptainShotgun");

            // EntityStates.Commando.CommandoWeapon.FireShotgunBlast
            initIdentifier("TracerCommandoShotgun", "HitsparkCommandoShotgun");

            // EntityStates.Huntress.Weapon.FireArrowSnipe
            initIdentifier("TracerHuntressSnipe", "OmniImpactVFXHuntress");

            // EntityStates.Railgunner.Weapon.FireSnipeCryo
            initIdentifier("TracerRailgunCryo", "ImpactRailgun", DamageType.Freeze2s, BulletAttackFlags.Sniper);

            // EntityStates.Railgunner.Weapon.FireSnipeHeavy
            initIdentifier("TracerRailgun", "ImpactRailgun", DamageType.Generic, BulletAttackFlags.Sniper);

            // EntityStates.Railgunner.Weapon.FireSnipeLight
            initIdentifier("TracerRailgunLight", "ImpactRailgunLight", DamageType.Generic, BulletAttackFlags.Sniper);

            // EntityStates.Railgunner.Weapon.FireSnipeSuper
            initIdentifier("TracerRailgunSuper", "ImpactRailgun", DamageType.Generic, BulletAttackFlags.Sniper);

            // EntityStates.Toolbot.FireSpear
            initIdentifier("TracerToolbotRebar", "ImpactSpear");

            // EntityStates.GoldGat.GoldGatFire
            initIdentifier("TracerGoldGat", null);

            // EntityStates.LaserTurbine.FireMainBeamState
            initIdentifier("TracerLaserTurbine", null);
            initIdentifier("TracerLaserTurbineReturn", null);

            // EntityStates.LemurianBruiserMonster.Flamebreath
            // EntityStates.Mage.Weapon.Flamethrower
            initIdentifier(null, "MissileExplosionVFX");
            initIdentifier(null, "MissileExplosionVFX", DamageType.IgniteOnHit);

            // EntityStates.LunarWisp.FireLunarGuns
            initIdentifier("TracerLunarWispMinigun", "LunarWispMinigunHitspark");

            // EntityStates.Mage.Weapon.FireLaserbolt
            initIdentifier("TracerMageLightningLaser", "LightningFlash");

            // EntityStates.Toolbot.BaseNailgunState
            initIdentifier("TracerToolbotNails", "ImpactNailgun");

            // EntityStates.VoidRaidCrab.SpinBeamAttack
            initIdentifier(null, "LaserImpactEffect");

            // EntityStates.VoidSurvivor.Weapon.FireCorruptHandBeam
            initIdentifier(null, "VoidSurvivorBeamImpactCorrupt");

            // EntityStates.VoidSurvivor.Weapon.FireHandBeam
            initIdentifier("VoidSurvivorBeamTracer", "VoidSurvivorBeamImpact", DamageType.SlowOnHit);

            // EntityStates.Wisp1Monster.FireEmbers
            initIdentifier("TracerEmbers", "OmniImpactVFX");
            #endregion
        }

        protected override BulletAttackIdentifier InvalidIdentifier => BulletAttackIdentifier.Invalid;

        protected override BulletAttackIdentifier createIdentifierForObject(BulletAttack bulletAttack)
        {
            return new BulletAttackIdentifier(bulletAttack, BulletAttackFlags.None);
        }

        protected override NetworkMessageBase getSyncIdentifierNeededMessage(BulletAttackIdentifier identifier)
        {
            return new SyncBulletAttackIndexNeeded(identifier);
        }

        public override IEnumerable<NetworkMessageBase> GetNetMessages()
        {
            yield return new SyncBulletAttackCatalog(_identifiers, _identifiersCount);
        }

        public IEnumerable<ProjectileTypeIdentifier> GetAllBulletAttackProjectileIdentifiers()
        {
            return this.Select<BulletAttackIdentifier, ProjectileTypeIdentifier>(static i => i);
        }
    }
}
