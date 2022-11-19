using HG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.Utility
{
    public abstract class GenericCatalog<TObjects, TIdentifier> : IEnumerable<TIdentifier> where TIdentifier : ICatalogIdentifier<TObjects, TIdentifier>
    {
        static readonly string LOG_PREFIX_TYPE = $"{nameof(GenericCatalog<TObjects, TIdentifier>)}<{typeof(TObjects).Name}, {typeof(TIdentifier).Name}>";

        public event Action<TIdentifier> OnIdentifierAppended;

        protected int _identifiersCount = 0;
        protected TIdentifier[] _identifiers = new TIdentifier[20];

        protected virtual TIdentifier InvalidIdentifier => default;

        protected GenericCatalog()
        {
        }

        public TIdentifier GetIdentifier(int index)
        {
            if (index < 0 || index >= _identifiersCount)
                return InvalidIdentifier;

            return _identifiers[index];
        }

        protected void appendIdentifier(ref TIdentifier identifier, bool checkExisting)
        {
#if DEBUG
            string LOG_PREFIX = $"{LOG_PREFIX_TYPE}.{nameof(appendIdentifier)} ";
#endif

            if (checkExisting)
            {
                for (int i = 0; i < _identifiersCount; i++)
                {
                    if (_identifiers[i].Equals(identifier, false))
                    {
#if DEBUG
                        Log.Warning(LOG_PREFIX + $"duplicate attack identifier {identifier}");
#endif

                        return;
                    }
                }
            }

            identifier.Index = _identifiersCount;

#if DEBUG
            Log.Debug(LOG_PREFIX + $"appended {identifier}");
#endif

            ArrayUtils.ArrayAppend(ref _identifiers, ref _identifiersCount, identifier);

            OnIdentifierAppended?.Invoke(identifier);
        }

        protected bool tryGetAttackIdentifier(TObjects obj, out TIdentifier identifier)
        {
            foreach (TIdentifier existingIdentifier in _identifiers.Take(_identifiersCount))
            {
                if (existingIdentifier.Matches(obj))
                {
                    identifier = existingIdentifier;
                    return true;
                }
            }

            identifier = default;
            return false;
        }

        public virtual TIdentifier GetIdentifier(TObjects obj)
        {
            if (!tryGetAttackIdentifier(obj, out TIdentifier identifier))
            {
                return InvalidIdentifier;
            }

            return identifier;
        }

        public IEnumerator<TIdentifier> GetEnumerator()
        {
            return _identifiers.Take(_identifiersCount).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
