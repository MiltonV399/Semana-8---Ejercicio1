using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int maxSlots = 20;
    private List<InventorySlot> slots = new List<InventorySlot>();

    private Dictionary<string, List<InventorySlot>> itemsByType = new Dictionary<string, List<InventorySlot>>();

    private void Awake()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    public bool AddItem(BaseItem item, int amount = 1)
    {
        if (item.IsStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.Item != null && slot.Item.ItemId == item.ItemId && slot.Amount < item.MaxStack)
                {
                    int spaceLeft = item.MaxStack - slot.Amount;
                    int toAdd = Mathf.Min(spaceLeft, amount);
                    slot.Amount += toAdd;
                    amount -= toAdd;

                    UpdateItemsByType(item.ItemType, slot);

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        foreach (var slot in slots)
        {
            if (slot.Item == null)
            {
                slot.Item = item;
                slot.Amount = amount;
                UpdateItemsByType(item.ItemType, slot);

                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.LogWarning("Inventario lleno");
        return false;
    }

    public bool RemoveItem(BaseItem item, int amount = 1)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].Item == item)
            {
                slots[i].Amount -= amount;

                if (slots[i].Amount <= 0)
                {
                    RemoveFromItemsByType(item.ItemType, slots[i]);
                    slots[i].Item = null;
                    slots[i].Amount = 0;
                }

                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    private void UpdateItemsByType(string itemType, InventorySlot slot)
    {
        if (!itemsByType.ContainsKey(itemType))
        {
            itemsByType[itemType] = new List<InventorySlot>();
        }

        if (!itemsByType[itemType].Contains(slot))
        {
            itemsByType[itemType].Add(slot);
        }
    }

    private void RemoveFromItemsByType(string itemType, InventorySlot slot)
    {
        if (itemsByType.ContainsKey(itemType))
        {
            itemsByType[itemType].Remove(slot);

            if (itemsByType[itemType].Count == 0)
            {
                itemsByType.Remove(itemType);
            }
        }
    }

    public List<InventorySlot> GetItemsByType(string itemType)
    {
        return itemsByType.ContainsKey(itemType) ? new List<InventorySlot>(itemsByType[itemType]) : new List<InventorySlot>();
    }

    public List<InventorySlot> GetAllSlots()
    {
        return new List<InventorySlot>(slots);
    }

    public int GetItemCount(string itemId)
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.Item != null && slot.Item.ItemId == itemId)
            {
                count += slot.Amount;
            }
        }
        return count;
    }

    public event System.Action OnInventoryChanged;
}

[System.Serializable]
public class InventorySlot
{
    public BaseItem Item;
    public int Amount;

    public InventorySlot()
    {
        Item = null;
        Amount = 0;
    }
}