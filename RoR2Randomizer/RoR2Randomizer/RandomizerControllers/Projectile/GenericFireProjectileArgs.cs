using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Projectile
{
    public struct GenericFireProjectileArgs : ISerializableObject
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

        public TeamIndex OwnerTeam => TeamComponent.GetObjectTeam(Owner);

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

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            writer.Write(Owner);

            writer.Write(DamageType.HasValue);
            if (DamageType.HasValue)
                writer.Write(DamageType.Value);

            HurtBoxReference.FromHurtBox(Target).Write(writer);

            writer.Write(Weapon);

            writer.Write(MuzzleName);
        }

        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            Owner = reader.ReadGameObject();

            if (reader.ReadBoolean())
                DamageType = reader.ReadDamageType();

            HurtBoxReference hurtBoxReference = new HurtBoxReference();
            hurtBoxReference.Read(reader);
            Target = hurtBoxReference.ResolveHurtBox();

            Weapon = reader.ReadGameObject();

            MuzzleName = reader.ReadString();
        }
    }
}
