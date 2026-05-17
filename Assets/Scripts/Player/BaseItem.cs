using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class BaseItem : ScriptableObject, IItem
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private string itemType;
    [SerializeField] private bool isConsumable = true;
    [SerializeField] private bool isStackable = true;
    [SerializeField] private int maxStack = 99;
    [SerializeField] private Sprite icon;
    [SerializeField] private string description;

    public string ItemId => itemId;
    public string ItemName => itemName;
    public string ItemType => itemType;
    public bool IsConsumable => isConsumable;
    public bool IsStackable => isStackable;
    public int MaxStack => maxStack;
    public Sprite Icon => icon;
    public string Description => description;

    public virtual void OnEquip(PlayerCharacter player)
    {
        Debug.Log($"{itemName} equipado");
    }

    public virtual void OnUnequip(PlayerCharacter player)
    {
        Debug.Log($"{itemName} desequipado");
    }
}