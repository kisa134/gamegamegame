using Game.Domain.Characters;
using Game.Domain.Exceptions;
using Xunit;

namespace Game.Tests;

public sealed class CharacterTests
{
    [Fact]
    public void NewCharacter_StartsAtFullHealthAndAlive()
    {
        var c = new Character("Alaric", 100);
        Assert.Equal(100, c.Health);
        Assert.True(c.IsAlive);
    }

    [Fact]
    public void ApplyDamage_ReducesHealth()
    {
        var c = new Character("Alaric", 100);
        var killed = c.ApplyDamage(30);
        Assert.Equal(70, c.Health);
        Assert.False(killed);
    }

    [Fact]
    public void ApplyDamage_BeyondZero_ClampsAndKills()
    {
        var c = new Character("Alaric", 100);
        var killed = c.ApplyDamage(999);
        Assert.Equal(0, c.Health);
        Assert.False(c.IsAlive);
        Assert.True(killed);
    }

    [Fact]
    public void ApplyDamage_OnDead_DoesNothing()
    {
        var c = new Character("Alaric", 10);
        c.ApplyDamage(10);
        var killed = c.ApplyDamage(5);
        Assert.Equal(0, c.Health);
        Assert.False(killed);
    }

    [Fact]
    public void ApplyDamage_Negative_Throws() =>
        Assert.Throws<DomainException>(() => new Character("X", 10).ApplyDamage(-1));

    [Fact]
    public void Heal_CapsAtMaxHealth()
    {
        var c = new Character("Bri", 50);
        c.ApplyDamage(20);
        c.Heal(100);
        Assert.Equal(50, c.Health);
    }

    [Fact]
    public void Heal_OnDead_Throws()
    {
        var c = new Character("X", 10);
        c.ApplyDamage(10);
        Assert.Throws<DomainException>(() => c.Heal(5));
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData("  ", 10)]
    [InlineData("Ok", 0)]
    [InlineData("Ok", -5)]
    public void Constructor_WithInvalidArgs_Throws(string name, int maxHp) =>
        Assert.Throws<DomainException>(() => new Character(name, maxHp));
}
