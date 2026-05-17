using UnityEngine;

public interface IPlayer
{
    string PlayerName { get; }
    int CurrentHealth { get; }
    int MaxHealth { get; }
    int AttackPower { get; }
    bool IsAlive { get; }

    void TakeDamage(int damage);
    void Heal(int amount);
    void Attack(IDamageable target);
    void ShowPortrait();
    void Speak(string text);
}

public interface IDamageable
{
    void TakeDamage(int damage);
    bool IsAlive { get; }
}