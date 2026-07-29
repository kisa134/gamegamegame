using System.Numerics;
using Game.Domain.Exceptions;
using Game.Domain.World;
using Xunit;

namespace Game.Tests;

public sealed class TreeTests
{
    private static Tree Oak() => new("oak-1", Vector3.Zero, "wood", 3, 1f);

    [Fact]
    public void NewTree_StandsAndKnowsItsYield()
    {
        var tree = Oak();

        Assert.False(tree.IsFelled);
        Assert.Equal("wood", tree.YieldItemId);
        Assert.Equal(3, tree.YieldAmount);
    }

    [Fact]
    public void Fell_PutsTheTreeDown()
    {
        var tree = Oak();

        tree.Fell();

        Assert.True(tree.IsFelled);
    }

    [Fact]
    public void Fell_Twice_Throws()
    {
        var tree = Oak();
        tree.Fell();

        Assert.Throws<DomainException>(() => tree.Fell());
    }

    [Theory]
    [InlineData("", "wood", 1)]
    [InlineData("oak-1", "", 1)]
    [InlineData("oak-1", "wood", 0)]
    [InlineData("oak-1", "wood", -2)]
    public void Constructor_WithInvalidArgs_Throws(string id, string itemId, int amount) =>
        Assert.Throws<DomainException>(() => new Tree(id, Vector3.Zero, itemId, amount, 1f));
}
