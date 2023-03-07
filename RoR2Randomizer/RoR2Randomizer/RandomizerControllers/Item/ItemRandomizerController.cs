#if !DISABLE_ITEM_RANDOMIZER
using HG;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.PrefabMarkers;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Item
{
    [RandomizerController]
    public class ItemRandomizerController : BaseRandomizerController
    {
        static ItemRandomizerController _instance;

        static int[] _pickupIndicesToRandomize;

        [SystemInitializer(typeof(PickupCatalog), typeof(NullModelMarker))]
        static void Init()
        {
            const string LOG_PREFIX = $"{nameof(ItemRandomizerController)}.{nameof(Init)} ";

            _pickupIndicesToRandomize = PickupCatalog.allPickups
                                                     .Where(static pd =>
                                                     {
                                                         if (pd.artifactIndex != ArtifactIndex.None)
                                                         {
#if DEBUG
                                                             Log.Debug($"excluding artifact pickup {pd.pickupIndex}");
#endif
                                                             return false;
                                                         }

                                                         if (pd.miscPickupIndex != MiscPickupIndex.None)
                                                         {
#if DEBUG
                                                             Log.Debug($"excluding misc pickup {pd.pickupIndex}");
#endif
                                                             return false;
                                                         }

                                                         if (pd.internalName.StartsWith("ItemTier."))
                                                         {
#if DEBUG
                                                             Log.Debug($"excluding {nameof(ItemTier)} pickup {pd.pickupIndex}");
#endif
                                                             return false;
                                                         }

                                                         if (pd.equipmentIndex != EquipmentIndex.None && ReadOnlyArray<EquipmentIndex>.BinarySearch(CharacterReplacements.AvailableDroneEquipments, pd.equipmentIndex) < 0)
                                                         {
#if DEBUG
                                                             Log.Debug($"excluding invalid equipment pickup {pd.pickupIndex}");
#endif
                                                             return false;
                                                         }

                                                         if (!pd.dropletDisplayPrefab)
                                                         {
#if DEBUG
                                                             Log.Debug($"excluding pickup {pd.pickupIndex} due to: null {nameof(PickupDef.dropletDisplayPrefab)}");
#endif
                                                             return false;
                                                         }

                                                         if (!pd.displayPrefab || pd.displayPrefab.GetComponent<NullModelMarker>())
                                                         {
#if DEBUG
                                                             Log.Debug($"excluding pickup {pd.pickupIndex} due to: null {nameof(PickupDef.displayPrefab)}");
#endif
                                                             return false;
                                                         }

                                                         if (string.IsNullOrEmpty(pd.nameToken) || Language.GetString(pd.nameToken) == pd.nameToken)
                                                         {
#if DEBUG
                                                             Log.Debug($"excluding pickup {pd.pickupIndex} due to: invalid name token {pd.nameToken}");
#endif
                                                             return false;
                                                         }

                                                         return true;
                                                     })
                                                     .Select(static pd => pd.pickupIndex.value)
                                                     .ToArray();
        }

        public override bool IsRandomizerEnabled => IsEnabled;

        public static bool IsEnabled => NetworkServer.active && ConfigManager.ItemRandomizer.Enabled;

        static readonly RunSpecific<IndexReplacementsCollection> _pickupIndexReplacements = new RunSpecific<IndexReplacementsCollection>(static (out IndexReplacementsCollection result) =>
        {
            if (IsEnabled)
            {
                result = new IndexReplacementsCollection(ReplacementDictionary<int>.CreateFrom(_pickupIndicesToRandomize, _instance.RNG), PickupCatalog.pickupCount);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        });

        protected override bool isNetworked => true;

        protected override IEnumerable<NetworkMessageBase> getNetMessages()
        {
            Log.Warning("TODO: Add item randomizer net message");
            yield break;
        }

        protected override void Awake()
        {
            base.Awake();

            SingletonHelper.Assign(ref _instance, this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _pickupIndexReplacements.Dispose();

            SingletonHelper.Unassign(ref _instance, this);
        }

        public static bool TryGetReplacementPickupIndex(in PickupIndex original, out PickupIndex replacement)
        {
            if (IsEnabled &&
                original.isValid &&
                _pickupIndexReplacements.HasValue &&
                _pickupIndexReplacements.Value.TryGetReplacement(original.value, out int replacementIndex))
            {
                replacement = new PickupIndex(replacementIndex);
#if DEBUG
                Log.Debug($"Replaced pickup: {original} -> {replacement}");
#endif
                return replacement.isValid;
            }

            replacement = PickupIndex.none;
            return false;
        }

        public static PickupIndex GetReplacementPickupIndex(PickupIndex pickupIndex)
        {
            if (TryGetReplacementPickupIndex(pickupIndex, out PickupIndex replacement))
            {
                return replacement;
            }
            else
            {
                return pickupIndex;
            }
        }
    }
}
#endif