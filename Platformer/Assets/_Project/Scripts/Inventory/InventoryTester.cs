using UnityEngine;
namespace Platformer
{
    public class InventoryTester : MonoBehaviour
    {
        [SerializeField] Inventory inventory;
        [SerializeField] ItemData testItem;
        [SerializeField] int qty = 1;

        [ContextMenu("Add")]    void Add()    => inventory.Add(testItem, qty);
        [ContextMenu("Remove")] void Remove() => inventory.Remove(testItem, qty);
    }
}