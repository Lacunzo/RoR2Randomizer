using RoR2;
using RoR2Randomizer.Networking.Generic.Chunking;
using RoR2Randomizer.RandomizerControllers.Item_Tier;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace RoR2Randomizer.Networking.ItemTierRandomizer
{
    public sealed class SyncItemTierReplacements : ChunkedNetworkMessage
    {
        public delegate void OnReceiveDelegate(ItemTier?[] itemTierOverrides);
        public static event OnReceiveDelegate OnReceive;

        ItemTier?[] _itemTierOverrides;

        public SyncItemTierReplacements(ItemTier?[] itemTierOverrides)
        {
            _itemTierOverrides = itemTierOverrides;
        }

        public SyncItemTierReplacements()
        {
        }

        public override void Serialize(NetworkWriter writer)
        {
            List<KeyValuePair<ItemIndex, ItemTier>> tierReplacements = new List<KeyValuePair<ItemIndex, ItemTier>>();

            foreach (ItemIndex index in ItemCatalog.allItems)
            {
                ItemTier? tierOverride = _itemTierOverrides[(int)index];
                if (!tierOverride.HasValue)
                    continue;

                tierReplacements.Add(new KeyValuePair<ItemIndex, ItemTier>(index, tierOverride.Value));
            }

            writer.WritePackedUInt32((uint)tierReplacements.Count);

            foreach (KeyValuePair<ItemIndex, ItemTier> itemTierReplacement in tierReplacements)
            {
                writer.WritePackedUInt32((uint)itemTierReplacement.Key);
                writer.Write((byte)itemTierReplacement.Value);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            _itemTierOverrides = ItemCatalog.GetPerItemBuffer<ItemTier?>();

            uint tierOverridesCount = reader.ReadPackedUInt32();
            for (int i = 0; i < tierOverridesCount; i++)
            {
                uint itemIndex = reader.ReadPackedUInt32();
                ItemTier overrideTier = (ItemTier)reader.ReadByte();

                _itemTierOverrides[itemIndex] = overrideTier;
            }
        }

        public override void OnReceived()
        {
            OnReceive?.Invoke(_itemTierOverrides);
        }
    }
}
