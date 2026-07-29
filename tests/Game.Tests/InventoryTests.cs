using Game.Domain.Exceptions;
using Game.Domain.Inventory;
using Xunit;

namespace Game.Tests;

public sealed class InventoryTests
{
    private const string Wood = "wood";
    private const float WoodWeight = 1.5f;

    [Fact]
    public void AddItem_WhenWithinCapacity_IncreasesQuantityAndWeight()
    {
        var inventory = new Inventory(maxWeight: 10f);

        inventory.AddItem(Wood, quantity: 2, unitWeight: WoodWeight);

        Assert.Equal(2, inventory.GetQuantity(Wood));
        Assert.Equal(3f, inventory.CurrentWeight);
        Assert.Equal(7f, inventory.RemainingCapacity);
    }

    [Fact]
    public void AddItem_WhenWouldExceedCapacity_ThrowsAndLeavesStateUnchanged()
    {
        var inventory = new Inventory(maxWeight: 5f);
        inventory.AddItem(Wood, quantity: 2, unitWeight: WoodWeight);

        var ex = Assert.Throws<DomainException>(() =>
            inventory.AddItem(Wood, quantity: 2, unitWeight: WoodWeight));

        Assert.Contains("exceed max weight", ex.Message);
        Assert.Equal(2, inventory.GetQuantity(Wood));
        Assert.Equal(3f, inventory.CurrentWeight);
    }

    [Fact]
    public void RemoveItem_WhenItemMissing_Throws()
    {
        var inventory = new Inventory(maxWeight: 10f);

        var ex = Assert.Throws<DomainException>(() =>
            inventory.RemoveItem(Wood, quantity: 1));

        Assert.Contains("only 0 available", ex.Message);
        Assert.Equal(0, inventory.GetQuantity(Wood));
        Assert.Equal(0f, inventory.CurrentWeight);
    }

    [Fact]
    public void RemoveItem_WhenQuantityTooHigh_ThrowsAndLeavesStateUnchanged()
    {
        var inventory = new Inventory(maxWeight: 10f);
        inventory.AddItem(Wood, quantity: 2, unitWeight: WoodWeight);

        Assert.Throws<DomainException>(() =>
            inventory.RemoveItem(Wood, quantity: 3));

        Assert.Equal(2, inventory.GetQuantity(Wood));
        Assert.Equal(3f, inventory.CurrentWeight);
    }

    [Fact]
    public void RemoveItem_WhenEnough_DecreasesQuantityAndWeight()
    {
        var inventory = new Inventory(maxWeight: 10f);
        inventory.AddItem(Wood, quantity: 3, unitWeight: WoodWeight);

        inventory.RemoveItem(Wood, quantity: 2);

        Assert.Equal(1, inventory.GetQuantity(Wood));
        Assert.Equal(1.5f, inventory.CurrentWeight);
    }

    [Fact]
    public void Constructor_WhenMaxWeightNonPositive_Throws()
    {
        Assert.Throws<DomainException>(() => new Inventory(maxWeight: 0f));
        Assert.Throws<DomainException>(() => new Inventory(maxWeight: -1f));
    }
}
