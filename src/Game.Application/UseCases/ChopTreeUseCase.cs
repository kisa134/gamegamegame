using System.Numerics;
using Game.Application.Events;
using Game.Application.Ports;
using Game.Compat;
using Game.Domain.Events;
using InventoryBag = Game.Domain.Inventory.Inventory;

namespace Game.Application.UseCases;

/// <summary>What an actor intends: swing at whatever tree is in front of them.</summary>
public readonly record struct ChopTreeCommand(Vector3 Origin, float Reach);

/// <summary>
/// Turns the intention "chop" into the fact "a tree was chopped".
/// Player and NPC both call this — there is deliberately no way to tell them apart,
/// because a divergence here is the AI Consistency gate failing.
/// </summary>
public sealed class ChopTreeUseCase
{
    private readonly ITreeRegistry _trees;
    private readonly IEventBus _bus;

    public ChopTreeUseCase(ITreeRegistry trees, IEventBus bus)
    {
        _trees = Guard.NotNull(trees, nameof(trees));
        _bus = Guard.NotNull(bus, nameof(bus));
    }

    /// <summary>
    /// Returns false when there was simply nothing in reach — a miss is ordinary, not an error.
    /// Throws when the actor could not carry the yield, and in that case the tree is left
    /// standing: the wood goes into the bag before the tree comes down, so a full inventory
    /// cannot destroy a tree for nothing.
    /// </summary>
    public bool Execute(ChopTreeCommand command, InventoryBag inventory)
    {
        Guard.NotNull(inventory, nameof(inventory));

        var tree = _trees.FindStandingTreeWithinReach(command.Origin, command.Reach);
        if (tree is null)
        {
            return false;
        }

        inventory.AddItem(tree.YieldItemId, tree.YieldAmount, tree.YieldUnitWeight);
        tree.Fell();

        _bus.Publish(new TreeChopped(tree.Id, tree.YieldItemId, tree.YieldAmount));
        return true;
    }
}
