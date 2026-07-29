using System.Numerics;
using Game.Domain.Exceptions;
using InventoryBag = Game.Domain.Inventory.Inventory;

namespace Game.Domain.Building;

/// <summary>
/// Rich entity: a placed building piece. A piece cannot exist in the air —
/// construction requires support (ground or an existing piece) — and it cannot
/// exist unpaid: its cost leaves the builder's inventory as it is created.
/// Coordinates use System.Numerics.Vector3, never UnityEngine.Vector3 (Gate A).
/// </summary>
public sealed class BuildingPiece
{
    public string Kind { get; }
    public Vector3 Position { get; }
    public SupportKind Support { get; }

    private BuildingPiece(string kind, Vector3 position, SupportKind support)
    {
        Kind = kind;
        Position = position;
        Support = support;
    }

    /// <summary>
    /// The only way to create a piece. Validates kind, then support, then affordability,
    /// in that order, so each refusal has one unambiguous cause. A refused build leaves
    /// the inventory exactly as it was.
    /// </summary>
    public static BuildingPiece Build(
        string kind,
        Vector3 position,
        SupportKind support,
        BuildingCost cost,
        InventoryBag inventory)
    {
        if (string.IsNullOrWhiteSpace(kind))
            throw new DomainException("Building piece kind is required.");
        if (support == SupportKind.None)
            throw new DomainException("Cannot place a building piece in the air: support required.");

        ArgumentNullException.ThrowIfNull(cost);
        ArgumentNullException.ThrowIfNull(inventory);

        // The whole cost is verified before anything is removed. Deducting as we go
        // would leave a half-paid inventory behind when a later item falls short.
        foreach (var (itemId, required) in cost.Requirements)
        {
            var available = inventory.GetQuantity(itemId);
            if (available < required)
                throw new DomainException(
                    $"Cannot place '{kind}': requires {required} of '{itemId}', inventory has {available}.");
        }

        foreach (var (itemId, required) in cost.Requirements)
        {
            inventory.RemoveItem(itemId, required);
        }

        return new BuildingPiece(kind, position, support);
    }
}
