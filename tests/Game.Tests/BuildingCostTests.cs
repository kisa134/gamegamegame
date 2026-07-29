using Game.Domain.Building;
using Game.Domain.Exceptions;
using Xunit;

namespace Game.Tests;

public sealed class BuildingCostTests
{
    [Fact]
    public void Free_RequiresNothing()
    {
        Assert.True(BuildingCost.Free.IsFree);
        Assert.Empty(BuildingCost.Free.Requirements);
    }

    [Fact]
    public void Of_KeepsEveryRequirement()
    {
        var cost = BuildingCost.Of(("wood", 3), ("stone", 1));

        Assert.False(cost.IsFree);
        Assert.Equal(3, cost.Requirements["wood"]);
        Assert.Equal(1, cost.Requirements["stone"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Of_WithNonPositiveQuantity_Throws(int quantity) =>
        Assert.Throws<DomainException>(() => BuildingCost.Of(("wood", quantity)));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Of_WithBlankItemId_Throws(string itemId) =>
        Assert.Throws<DomainException>(() => BuildingCost.Of((itemId, 1)));

    [Fact]
    public void Of_WithDuplicateItem_Throws() =>
        Assert.Throws<DomainException>(() => BuildingCost.Of(("wood", 1), ("wood", 2)));
}
