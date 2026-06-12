using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Player-owned bag of items. Handles stacking, capacity, and broadcasts changes
    /// via the global InventoryEvents bus. No UI logic here.
    /// </summary>
    public class Inventory : MonoBehaviour, IDataPersistence
    {
        [Header("Capacity (max distinct stacks)")]
        [SerializeField, Min(1)] int capacity = 50;

        [Header("Runtime State (visible for debugging)")]
        [SerializeField] List<InventoryItem> items = new();

        public IReadOnlyList<InventoryItem> Items => items;
        public int Capacity => capacity;
        public int UsedSlots => items.Count;
        public bool IsFull => items.Count >= capacity;

        /// <summary>Adds a quantity of an item, stacking into existing entries first. Returns true if the full amount fit.</summary>
        public bool Add(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            int remaining = quantity;

            // Fill existing stacks first (only matters for stackable items).
            if (item.maxStack > 1)
            {
                foreach (var existing in items)
                {
                    if (existing.data != item || existing.IsFull) continue;

                    int add = Mathf.Min(existing.Space, remaining);
                    existing.quantity += add;
                    remaining -= add;
                    Raise(InventoryEventType.Added, existing);

                    if (remaining == 0) return true;
                }
            }

            // Spill the rest into new stacks.
            while (remaining > 0)
            {
                if (IsFull) return false;

                int stackSize = Mathf.Min(item.maxStack, remaining);
                var newItem = new InventoryItem(item, stackSize);
                items.Add(newItem);
                remaining -= stackSize;
                Raise(InventoryEventType.Added, newItem);
            }

            return true;
        }

        /// <summary>Removes a quantity of an item. Returns true if the full amount was removed.</summary>
        public bool Remove(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;
            if (TotalOf(item) < quantity) return false;

            int remaining = quantity;
            for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (items[i].data != item) continue;

                if (items[i].quantity > remaining)
                {
                    items[i].quantity -= remaining;
                    Raise(InventoryEventType.Removed, items[i]);
                    remaining = 0;
                }
                else
                {
                    remaining -= items[i].quantity;
                    var removed = items[i];
                    items.RemoveAt(i);
                    Raise(InventoryEventType.Removed, removed);
                }
            }
            return true;
        }

        public bool Has(ItemData item, int quantity = 1) => TotalOf(item) >= quantity;

        public int TotalOf(ItemData item)
        {
            int total = 0;
            foreach (var entry in items)
                if (entry.data == item) total += entry.quantity;
            return total;
        }

        enum InventoryEventType { Added, Removed }

        void Raise(InventoryEventType type, InventoryItem item)
        {
            if (GameEventsManager.instance == null) return;
            var bus = GameEventsManager.instance.inventoryEvents;
            if (type == InventoryEventType.Added) bus.ItemAdded(item);
            else bus.ItemRemoved(item);
        }

        // ─── IDataPersistence ──────────────────────────────────────────────────

        public void SaveData(GameData data)
        {
            data.inventoryItems = new List<InventoryEntry>(items.Count);
            foreach (var item in items)
            {
                if (item?.data == null || string.IsNullOrEmpty(item.data.id)) continue;
                data.inventoryItems.Add(new InventoryEntry
                {
                    itemId = item.data.id,
                    quantity = item.quantity
                });
            }
        }

        public void LoadData(GameData data)
        {
            items.Clear();
            if (data.inventoryItems == null) return;

            var db = ItemDatabase.instance;
            if (db == null)
            {
                Debug.LogWarning("ItemDatabase missing — inventory load skipped. " +
                                 "Create one at Assets/Resources/ItemDatabase.asset.");
                return;
            }

            foreach (var entry in data.inventoryItems)
            {
                var itemData = db.Find(entry.itemId);
                if (itemData != null)
                    items.Add(new InventoryItem(itemData, entry.quantity));
                else
                    Debug.LogWarning($"Inventory load: item id '{entry.itemId}' not found in ItemDatabase.");
            }
        }
    }
}
