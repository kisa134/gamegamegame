using System.Numerics;
using Game.Domain.Building;
using Game.Domain.Exceptions;
using Xunit;
using InventoryBag = Game.Domain.Inventory.Inventory;

namespace Game.Tests;

public sealed class BuildingPieceTests
{
    private const float Plenty = 1000f;

    private static InventoryBag Stocked(params (string ItemId, int Quantity)[] items)
    {
        var inventory = new InventoryBag(Plenty);
        foreach (var (itemId, quantity) in items)
        {
            inventory.AddItem(itemId, quantity, 1f);
        }

        return inventory;
    }

    [Fact]
    public void Build_WithSupportAndNoCost_Succeeds()
    {
        var piece = BuildingPiece.Build("wall", new Vector3(1, 0, 2), SupportKind.Ground, BuildingCost.Free, Stocked());

        Assert.Equal("wall", piece.Kind);
        Assert.Equal(SupportKind.Ground, piece.Support);
    }

    [Fact]
    public void Build_WithoutSupport_ThrowsNoAirPlacement() =>
        Assert.Throws<DomainException>(() =>
            BuildingPiece.Build("wall", Vector3.Zero, SupportKind.None, BuildingCost.Free, Stocked()));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_WithEmptyKind_Throws(string kind) =>
        Assert.Throws<DomainException>(() =>
            BuildingPiece.Build(kind, Vector3.Zero, SupportKind.Ground, BuildingCost.Free, Stocked()));

    [Fact]
    public void Build_WhenAffordable_DeductsExactlyTheCost()
    {
        var inventory = Stocked(("wood", 10), ("stone", 4));
        var cost = BuildingCost.Of(("wood", 3), ("stone", 1));

        BuildingPiece.Build("wall", Vector3.Zero, SupportKind.Ground, cost, inventory);

        Assert.Equal(7, inventory.GetQuantity("wood"));
        Assert.Equal(3, inventory.GetQuantity("stone"));
    }

    [Fact]
    public void Build_WithoutEnoughResources_ThrowsAndLeavesInventoryUntouched()
    {
        var inventory = Stocked(("wood", 2));
        var cost = BuildingCost.Of(("wood", 3));

        Assert.Throws<DomainException>(() =>
            BuildingPiece.Build("wall", Vector3.Zero, SupportKind.Ground, cost, inventory));

        Assert.Equal(2, inventory.GetQuantity("wood"));
    }

    [Fact]
    public void Build_WhenOnlyOneRequirementFallsShort_RemovesNothingAtAll()
    {
        var inventory = Stocked(("wood", 10), ("stone", 1));
        var cost = BuildingCost.Of(("wood", 3), ("stone", 5));

        Assert.Throws<DomainException>(() =>
            BuildingPiece.Build("wall", Vector3.Zero, SupportKind.Ground, cost, inventory));

        Assert.Equal(10, inventory.GetQuantity("wood"));
        Assert.Equal(1, inventory.GetQuantity("stone"));
    }

    [Fact]
    public void Build_WithMissingItemEntirely_Throws()
    {
        var inventory = Stocked(("wood", 10));

        Assert.Throws<DomainException>(() =>
            BuildingPiece.Build("wall", Vector3.Zero, SupportKind.Ground, BuildingCost.Of(("nails", 1)), inventory));
    }

    [Fact]
    public void Build_WhenBrokeAndInTheAir_ReportsSupportFirstAndKeepsResources()
    {
        var inventory = Stocked(("wood", 1));
        var cost = BuildingCost.Of(("wood", 99));

        var error = Assert.Throws<DomainException>(() =>
            BuildingPiece.Build("wall", Vector3.Zero, SupportKind.None, cost, inventory));

        Assert.Contains("air", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, inventory.GetQuantity("wood"));
    }
}
