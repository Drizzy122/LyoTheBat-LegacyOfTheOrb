using System;

namespace Platformer
{
    /// <summary>
    /// Runtime wrapper around an ItemData asset. Carries a mutable stack quantity
    /// so the same ItemData asset can represent multiple stacks in the inventory.
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public ItemData data;
        public int quantity;

        public InventoryItem(ItemData data, int quantity = 1)
        {
            this.data = data;
            this.quantity = quantity;
        }

        public bool IsFull => data == null || quantity >= data.maxStack;
        public int Space => data == null ? 0 : data.maxStack - quantity;
    }
}
