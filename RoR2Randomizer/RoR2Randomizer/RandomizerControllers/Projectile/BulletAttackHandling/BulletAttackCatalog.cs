using HG;
using R2API.Networking;
using RoR2;
using RoR2Randomizer.Networking;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Networking.ProjectileRandomizer;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling
{
    // Not identified by this method:
    // EntityStates.TitanMonster.FireMegaLaser
    // EntityStates.TitanMonster.FireGoldMegaLaser

    public class BulletAttackCatalog : INetMessageProvider
    {
        static BulletAttackCatalog _instance;

        [SystemInitializer(typeof(EffectCatalog))]
        static void Init()
        {
            _instance = new BulletAttackCatalog();
            NetworkingManager.RegisterMessageProvider(_instance, MessageProviderFlags.Persistent);

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

                appendIdentifier(ref identifier, IS_DEBUG);
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

            SyncBulletAttackCatalog.OnReceive += SyncBulletAttackCatalog_OnReceive;
            SyncBulletAttackIndexNeeded.OnReceive += SyncBulletAttackIndexNeeded_OnReceive;
        }

        public static event Action<BulletAttackIdentifier> BulletAttackAppended;

        static int _attackIdentifierCount = 0;
        static BulletAttackIdentifier[] _attackIdentifiers = new BulletAttackIdentifier[20];

        public static BulletAttackIdentifier GetBulletAttack(int index)
        {
            if (index < 0 || index >= _attackIdentifierCount)
                return BulletAttackIdentifier.Invalid;

            return _attackIdentifiers[index];
        }

        static void appendIdentifier(ref BulletAttackIdentifier identifier, bool checkExisting)
        {
            const string LOG_PREFIX = $"{nameof(BulletAttackCatalog)}.{nameof(appendIdentifier)} ";

            if (checkExisting)
            {
                for (int i = 0; i < _attackIdentifierCount; i++)
                {
                    if (_attackIdentifiers[i].Matches(identifier, false))
                    {
#if DEBUG
                        Log.Warning(LOG_PREFIX + $"duplicate attack identifier {identifier}");
#endif

                        return;
                    }
                }
            }

            identifier.Index = _attackIdentifierCount;

#if DEBUG
            Log.Debug(LOG_PREFIX + $"appended {identifier}");
#endif

            ArrayUtils.ArrayAppend(ref _attackIdentifiers, ref _attackIdentifierCount, identifier);

            BulletAttackAppended?.Invoke(identifier);
        }

        static bool tryGetAttackIdentifier(BulletAttack bulletAttack, out BulletAttackIdentifier identifier)
        {
            for (int i = 0; i < _attackIdentifierCount; i++)
            {
                if (_attackIdentifiers[i].Matches(bulletAttack))
                {
                    identifier = _attackIdentifiers[i];
                    return true;
                }
            }

            identifier = default;
            return false;
        }

        public static BulletAttackIdentifier GetBulletAttackIdentifier(BulletAttack bulletAttack)
        {
            if (!tryGetAttackIdentifier(bulletAttack, out BulletAttackIdentifier identifier))
            {
                identifier = new BulletAttackIdentifier(bulletAttack, BulletAttackFlags.None);
                if (NetworkServer.active)
                {
                    appendIdentifier(ref identifier, false);

#if DEBUG
                    Log.Debug($"Created {nameof(BulletAttackIdentifier)} '{identifier}'");
#endif

                    if (!NetworkServer.dontListen)
                    {
                        _instance.TrySendAll(NetworkDestination.Clients);
                    }
                }
                else
                {
                    if (NetworkClient.active)
                    {
                        new SyncBulletAttackIndexNeeded(identifier).SendTo(NetworkDestination.Server);
                    }

                    return BulletAttackIdentifier.Invalid;
                }
            }

            return identifier;
        }

        bool INetMessageProvider.SendMessages => true;

        IEnumerable<NetworkMessageBase> INetMessageProvider.GetNetMessages()
        {
            yield return new SyncBulletAttackCatalog(_attackIdentifiers, _attackIdentifierCount);
        }

        static void SyncBulletAttackCatalog_OnReceive(BulletAttackIdentifier[] identifiers, int identifiersCount)
        {
            if (!NetworkServer.active && NetworkClient.active)
            {
                _attackIdentifierCount = identifiersCount;

                ArrayUtils.EnsureCapacity(ref _attackIdentifiers, identifiersCount);
                Array.Copy(identifiers, _attackIdentifiers, identifiersCount);
            }
        }

        static void SyncBulletAttackIndexNeeded_OnReceive(BulletAttackIdentifier required)
        {
            if (NetworkServer.active)
            {
                appendIdentifier(ref required, true);
                _instance.TrySendAll(NetworkDestination.Clients);
            }
        }

        public static IEnumerable<ProjectileTypeIdentifier> GetAllBulletAttackProjectileIdentifiers()
        {
            for (int i = 0; i < _attackIdentifierCount; i++)
            {
                yield return _attackIdentifiers[i];
            }
        }
    }
}
