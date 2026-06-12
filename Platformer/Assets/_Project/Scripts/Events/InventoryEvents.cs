using System;

namespace Platformer
{
    public class InventoryEvents
    {
        public event Action<InventoryItem> onItemAdded;
        public void ItemAdded(InventoryItem item) => onItemAdded?.Invoke(item);

        public event Action<InventoryItem> onItemRemoved;
        public void ItemRemoved(InventoryItem item) => onItemRemoved?.Invoke(item);

        public event Action<InventoryItem, EquipSlot> onItemEquipped;
        public void ItemEquipped(InventoryItem item, EquipSlot slot) => onItemEquipped?.Invoke(item, slot);

        public event Action<InventoryItem, EquipSlot> onItemUnequipped;
        public void ItemUnequipped(InventoryItem item, EquipSlot slot) => onItemUnequipped?.Invoke(item, slot);
    }
}
