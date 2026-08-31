using System.Numerics;
using Game.Application.Events;
using Game.Application.Ports;
using Game.Application.UseCases;
using Game.Domain.Events;
using Game.Domain.Exceptions;
using Game.Domain.World;
using Xunit;
using InventoryBag = Game.Domain.Inventory.Inventory;

namespace Game.Tests;

public sealed class ChopTreeUseCaseTests
{
    /// <summary>Stands in for the Unity world. Hands back the tree only while it is standing.</summary>
    private sealed class FakeTreeRegistry : ITreeRegistry
    {
        private readonly Tree? _tree;

        public FakeTreeRegistry(Tree? tree) => _tree = tree;

        public Tree? FindStandingTreeWithinReach(Vector3 origin, float reach) =>
            _tree is { IsFelled: false } ? _tree : null;
    }

    private static Tree Oak(int yield = 3, float unitWeight = 1f) =>
        new("oak-1", Vector3.Zero, "wood", yield, unitWeight);

    private static ChopTreeCommand Swing => new(Vector3.Zero, 2f);

    [Fact]
    public void Chop_WhenATreeIsInReach_FillsTheBagFellsTheTreeAndPublishesTheFact()
    {
        var tree = Oak();
        var bus = new InMemoryEventBus();
        var inventory = new InventoryBag(100f);
        TreeChopped? published = null;
        bus.Subscribe<TreeChopped>(e => published = e);

        var chopped = new ChopTreeUseCase(new FakeTreeRegistry(tree), bus).Execute(Swing, inventory);

        Assert.True(chopped);
        Assert.Equal(3, inventory.GetQuantity("wood"));
        Assert.True(tree.IsFelled);
        Assert.NotNull(published);
        Assert.Equal("oak-1", published!.TreeId);
        Assert.Equal("wood", published.ItemId);
        Assert.Equal(3, published.Amount);
    }

    [Fact]
    public void Chop_WithNothingInReach_IsAnOrdinaryMiss()
    {
        var bus = new InMemoryEventBus();
        var inventory = new InventoryBag(100f);
        var published = 0;
        bus.Subscribe<TreeChopped>(_ => published++);

        var chopped = new ChopTreeUseCase(new FakeTreeRegistry(null), bus).Execute(Swing, inventory);

        Assert.False(chopped);
        Assert.Equal(0, published);
        Assert.Equal(0f, inventory.CurrentWeight);
    }

    [Fact]
    public void Chop_WhenTheBagIsTooFull_LeavesTheTreeStandingAndPublishesNothing()
    {
        var tree = Oak(yield: 10, unitWeight: 5f);
        var bus = new InMemoryEventBus();
        var inventory = new InventoryBag(10f);
        var published = 0;
        bus.Subscribe<TreeChopped>(_ => published++);

        var useCase = new ChopTreeUseCase(new FakeTreeRegistry(tree), bus);

        Assert.Throws<DomainException>(() => useCase.Execute(Swing, inventory));
        Assert.False(tree.IsFelled);
        Assert.Equal(0, inventory.GetQuantity("wood"));
        Assert.Equal(0, published);
    }

    [Fact]
    public void Chop_TheSameTreeTwice_FailsTheSecondTimeBecauseItIsAlreadyDown()
    {
        var tree = Oak();
        var registry = new FakeTreeRegistry(tree);
        var bus = new InMemoryEventBus();
        var useCase = new ChopTreeUseCase(registry, bus);
        var inventory = new InventoryBag(100f);

        Assert.True(useCase.Execute(Swing, inventory));
        Assert.False(useCase.Execute(Swing, inventory));
        Assert.Equal(3, inventory.GetQuantity("wood"));
    }

    [Fact]
    public void Chop_DoesNotDisturbSubscribersOfOtherEvents()
    {
        var bus = new InMemoryEventBus();
        var strangers = 0;
        bus.Subscribe<StructurePlaced>(_ => strangers++);
        bus.Subscribe<NightFell>(_ => strangers++);

        new ChopTreeUseCase(new FakeTreeRegistry(Oak()), bus).Execute(Swing, new InventoryBag(100f));

        Assert.Equal(0, strangers);
    }

    [Fact]
    public void Chop_ByAnNpcAndByThePlayer_TakeTheIdenticalPath()
    {
        var bus = new InMemoryEventBus();
        var playerBag = new InventoryBag(100f);
        var npcBag = new InventoryBag(100f);

        var player = new ChopTreeUseCase(new FakeTreeRegistry(Oak()), bus).Execute(Swing, playerBag);
        var npc = new ChopTreeUseCase(new FakeTreeRegistry(Oak()), bus).Execute(Swing, npcBag);

        Assert.Equal(player, npc);
        Assert.Equal(playerBag.GetQuantity("wood"), npcBag.GetQuantity("wood"));
    }
}
