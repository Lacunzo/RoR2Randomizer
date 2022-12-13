using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.BodyAnimationMirroring
{
    [Serializable]
    public readonly struct BoneIdentifier : IEquatable<BoneIdentifier>
    {
        [SerializeField]
        readonly bool _isID;

        [SerializeField]
        readonly HumanBodyBones _boneID;

        [SerializeField]
        readonly string _boneName;

        public BoneIdentifier(HumanBodyBones boneID) : this(true, boneID, null)
        {
        }

        public BoneIdentifier(string boneName) : this(false, (HumanBodyBones)(-1), boneName)
        {
        }

        BoneIdentifier(bool isID, HumanBodyBones boneID, string boneName)
        {
            _isID = isID;
            _boneID = boneID;
            _boneName = boneName;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is BoneIdentifier identifier && Equals(identifier);
        }

        public readonly bool Equals(BoneIdentifier other)
        {
            if (_isID != other._isID)
            {
                return false;
            }
            else if (_isID) // Both are equal, no need to check other._isID
            {
                return _boneID == other._boneID;
            }
            else
            {
                return string.Equals(_boneName, other._boneName, StringComparison.OrdinalIgnoreCase);
            }
        }

        public override readonly int GetHashCode()
        {
            int hashCode = -813282265;
            hashCode = (hashCode * -1521134295) + _isID.GetHashCode();

            if (_isID)
            {
                hashCode = (hashCode * -1521134295) + _boneID.GetHashCode();
            }
            else
            {
                hashCode = (hashCode * -1521134295) + _boneName.GetHashCode();
            }

            return hashCode;
        }

        public static bool operator ==(in BoneIdentifier left, in BoneIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in BoneIdentifier left, in BoneIdentifier right)
        {
            return !(left == right);
        }

        public static implicit operator BoneIdentifier(HumanBodyBones boneID)
        {
            return new BoneIdentifier(boneID);
        }
        public static implicit operator BoneIdentifier(string boneName)
        {
            return new BoneIdentifier(boneName);
        }
    }
}
