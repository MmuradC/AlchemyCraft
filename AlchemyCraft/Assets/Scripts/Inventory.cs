using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public const int SlotCount = 3;

    [Serializable]
    public class Slot
    {
        public InventoryItem item;
        public int amount;
        public bool IsEmpty => item == null;
    }

    public Slot[] slots = new Slot[SlotCount];
    public event Action OnInventoryChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        for (int i = 0; i < SlotCount; i++)
            slots[i] = new Slot();
    }

    /// <summary>Add item to first empty slot. Returns false if inventory full.</summary>
    public bool AddItem(InventoryItem item, int amount = 1)
    {
        // Try to stack into existing slot with same item
        foreach (Slot s in slots)
        {
            if (!s.IsEmpty && s.item == item)
            {
                s.amount += amount;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // Otherwise find empty slot
        foreach (Slot s in slots)
        {
            if (s.IsEmpty)
            {
                s.item   = item;
                s.amount = amount;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.LogWarning("Inventory full!");
        return false;
    }

    /// <summary>Remove one item from the given slot index.</summary>
    public void RemoveAt(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return;
        Slot s = slots[slotIndex];
        if (s.IsEmpty) return;

        s.amount -= amount;
        if (s.amount <= 0)
        {
            s.item   = null;
            s.amount = 0;
        }
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Clear a slot entirely.</summary>
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return;
        slots[slotIndex].item   = null;
        slots[slotIndex].amount = 0;
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Swap two slots (used by drag-and-drop UI).</summary>
    public void SwapSlots(int a, int b)
    {
        if (a == b) return;
        (slots[a], slots[b]) = (slots[b], slots[a]);
        OnInventoryChanged?.Invoke();
    }
}