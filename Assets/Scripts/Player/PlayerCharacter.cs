using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerCharacter : MonoBehaviour, IPlayer
{
    [SerializeField] private string playerName = "Renzo";
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackPower = 20;
    [SerializeField] private AssetReferenceSprite portrait2D;
    [SerializeField] private AssetReferenceGameObject portrait3D;
    [SerializeField] private PlayerInventory inventory;

    private int currentHealth;
    private Stack<BaseItem> recentItems = new Stack<BaseItem>();
    private Queue<BaseItem> itemQueue = new Queue<BaseItem>();
    private Dictionary<string, BaseItem> equippedItems = new Dictionary<string, BaseItem>();
    private List<BaseItem> allItems = new List<BaseItem>();

    public string PlayerName => playerName;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int AttackPower => attackPower;
    public bool IsAlive => currentHealth > 0;
    public PlayerInventory Inventory => inventory;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                inventory = gameObject.AddComponent<PlayerInventory>();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"{playerName} recibe {damage} de dano. Vida: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (!IsAlive)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;

        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        int healed = currentHealth - oldHealth;

        Debug.Log($"{playerName} se cura {healed} de vida. Vida: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log($"{playerName} ha muerto");
        OnPlayerDied?.Invoke();
        HidePortrait();
    }

    public void Attack(IDamageable target)
    {
        if (!IsAlive)
        {
            Debug.Log($"{playerName} no puede atacar porque esta muerto");
            return;
        }

        if (target == null || !target.IsAlive)
        {
            Debug.Log("El objetivo no es valido");
            return;
        }

        int totalDamage = attackPower;

        foreach (var item in equippedItems.Values)
        {
            if (item is IWeapon weapon)
            {
                totalDamage += weapon.DamageBonus;
            }
        }

        Debug.Log($"{playerName} ataca causando {totalDamage} de dano");
        target.TakeDamage(totalDamage);
        OnPlayerAttacked?.Invoke(totalDamage);
    }

    public void AddItem(BaseItem item)
    {
        if (item == null) return;

        recentItems.Push(item);
        itemQueue.Enqueue(item);
        allItems.Add(item);

        if (inventory != null)
        {
            inventory.AddItem(item);
        }

        Debug.Log($"{playerName} recogio: {item.ItemName}");
        OnItemAdded?.Invoke(item);
    }

    public BaseItem RemoveLastItem()
    {
        if (recentItems.Count > 0)
        {
            BaseItem item = recentItems.Pop();
            allItems.Remove(item);

            if (inventory != null)
            {
                inventory.RemoveItem(item);
            }

            Debug.Log($"{playerName} elimino: {item.ItemName}");
            return item;
        }
        return null;
    }

    public BaseItem GetNextItem()
    {
        return itemQueue.Count > 0 ? itemQueue.Peek() : null;
    }

    public void UseItem(BaseItem item)
    {
        if (item == null) return;

        if (item is IUsable usable)
        {
            usable.Use(this);

            if (item.IsConsumable)
            {
                RemoveItemFromCollections(item);
            }
        }
        else
        {
            Debug.Log($"{item.ItemName} no se puede usar directamente");
        }
    }

    public void EquipItem(BaseItem item)
    {
        if (item == null) return;

        if (equippedItems.ContainsKey(item.ItemType))
        {
            UnequipItem(equippedItems[item.ItemType]);
        }

        equippedItems[item.ItemType] = item;
        item.OnEquip(this);

        Debug.Log($"{playerName} equipo: {item.ItemName}");
        OnItemEquipped?.Invoke(item);
    }

    public void UnequipItem(BaseItem item)
    {
        if (item != null && equippedItems.ContainsValue(item))
        {
            equippedItems.Remove(item.ItemType);
            item.OnUnequip(this);
            Debug.Log($"{playerName} desequipo: {item.ItemName}");
            OnItemUnequipped?.Invoke(item);
        }
    }

    private void RemoveItemFromCollections(BaseItem item)
    {
        allItems.Remove(item);

        Stack<BaseItem> newStack = new Stack<BaseItem>();
        foreach (var i in recentItems)
        {
            if (i != item)
                newStack.Push(i);
        }
        recentItems = newStack;

        Queue<BaseItem> newQueue = new Queue<BaseItem>();
        foreach (var i in itemQueue)
        {
            if (i != item)
                newQueue.Enqueue(i);
        }
        itemQueue = newQueue;

        if (inventory != null)
        {
            inventory.RemoveItem(item);
        }
    }

    public List<BaseItem> GetAllItems()
    {
        return new List<BaseItem>(allItems);
    }

    public Dictionary<string, BaseItem> GetEquipment()
    {
        return new Dictionary<string, BaseItem>(equippedItems);
    }

    public Stack<BaseItem> GetRecentItems()
    {
        return new Stack<BaseItem>(recentItems);
    }

    public async void ShowPortrait()
    {
        if (portrait2D != null)
        {
            var handle = portrait2D.LoadAssetAsync<Sprite>();
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                OnPortrait2DReady?.Invoke(handle.Result);
            }
        }

        if (portrait3D != null)
        {
            var handle = portrait3D.InstantiateAsync();
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                OnPortrait3DReady?.Invoke(handle.Result);
            }
        }
    }

    public void HidePortrait()
    {
        OnPortraitHide?.Invoke();
    }

    public void Speak(string text)
    {
        Debug.Log($"{playerName}: {text}");
        OnPlayerSpeaks?.Invoke(playerName, text);
    }

    public System.Action<int, int> OnHealthChanged;
    public System.Action<int> OnPlayerAttacked;
    public System.Action OnPlayerDied;
    public System.Action<BaseItem> OnItemAdded;
    public System.Action<BaseItem> OnItemEquipped;
    public System.Action<BaseItem> OnItemUnequipped;
    public System.Action<Sprite> OnPortrait2DReady;
    public System.Action<GameObject> OnPortrait3DReady;
    public System.Action OnPortraitHide;
    public System.Action<string, string> OnPlayerSpeaks;
}