using UnityEngine;

public interface IItem
{
    string ItemId { get; }
    string ItemName { get; }
    string ItemType { get; }
    bool IsConsumable { get; }
    bool IsStackable { get; }
    int MaxStack { get; }
    Sprite Icon { get; }
}

public interface IUsable
{
    void Use(PlayerCharacter user);
}

public interface IWeapon
{
    int DamageBonus { get; }
}