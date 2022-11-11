using RoR2;
using UnityEngine;

namespace RoR2Randomizer.RandomizerControllers.Projectile
{
    public struct GenericFireProjectileArgs
    {
        public GameObject Owner;
        public CharacterBody OwnerBody
        {
            get
            {
                if (Owner)
                {
                    if (Owner.TryGetComponent<CharacterMaster>(out CharacterMaster master))
                    {
                        return master.GetBody();
                    }
                    else if (Owner.TryGetComponent<CharacterBody>(out CharacterBody body))
                    {
                        return body;
                    }
                }

                return null;
            }
        }

        public DamageType? DamageType;

        public HurtBox Target;
        public GameObject TargetGO
        {
            get
            {
                if (Target)
                    return Target.gameObject;

                return null;
            }
        }

        public GameObject Weapon;

        public string MuzzleName;

        public GenericFireProjectileArgs()
        {
        }

        public GenericFireProjectileArgs(BulletAttack bulletAttack)
        {
            Owner = bulletAttack.owner;
            DamageType = bulletAttack.damageType;
            Weapon = bulletAttack.weapon;
            MuzzleName = bulletAttack.muzzleName;
        }

        public void ModifyArgs(ref Vector3 position)
        {
            if (Weapon && !string.IsNullOrEmpty(MuzzleName))
            {
                ModelLocator modelLocator = Weapon.GetComponent<ModelLocator>();
                if (modelLocator && modelLocator.modelTransform)
                {
                    ChildLocator childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
                    if (childLocator)
                    {
                        Transform muzzle = childLocator.FindChild(MuzzleName);
                        if (muzzle)
                        {
                            position = muzzle.position;
                        }
                    }
                }
            }
        }
    }
}
