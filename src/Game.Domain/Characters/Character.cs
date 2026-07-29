using Game.Domain.Exceptions;

namespace Game.Domain.Characters;

/// <summary>
/// Rich entity: a living character (player or NPC). Health stays within [0, MaxHealth];
/// the dead take no damage. Fields are private; state changes only through guarded methods.
/// </summary>
public sealed class Character
{
    public string Name { get; }
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public bool IsAlive => Health > 0;

    public Character(string name, int maxHealth)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Character name is required.");
        if (maxHealth <= 0)
            throw new DomainException("Max health must be greater than zero.");

        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    /// <summary>Applies damage. Negative is rejected; the dead take none; health floors at 0.</summary>
    /// <returns>true if this blow killed the character.</returns>
    public bool ApplyDamage(int amount)
    {
        if (amount < 0)
            throw new DomainException("Damage cannot be negative.");
        if (!IsAlive)
            return false;

        Health = Math.Max(0, Health - amount);
        return !IsAlive;
    }

    /// <summary>Heals a living character up to MaxHealth. The dead cannot be healed.</summary>
    public void Heal(int amount)
    {
        if (amount < 0)
            throw new DomainException("Heal amount cannot be negative.");
        if (!IsAlive)
            throw new DomainException("Cannot heal a dead character.");

        Health = Math.Min(MaxHealth, Health + amount);
    }
}
