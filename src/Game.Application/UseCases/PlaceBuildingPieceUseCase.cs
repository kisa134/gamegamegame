using System.Numerics;
using Game.Application.Events;
using Game.Compat;
using Game.Domain.Building;
using Game.Domain.Events;
using InventoryBag = Game.Domain.Inventory.Inventory;

namespace Game.Application.UseCases;

/// <summary>What an actor intends: put a piece of this kind here, resting on that.</summary>
public readonly record struct PlaceBuildingPieceCommand(string Kind, Vector3 Position, SupportKind Support);

/// <summary>
/// Turns the intention "build" into the fact "a structure was placed".
/// The domain refuses unsupported or unaffordable placements and leaves the inventory
/// untouched when it does; this publishes only when a piece actually came into being.
/// </summary>
public sealed class PlaceBuildingPieceUseCase
{
    private readonly IEventBus _bus;
    private readonly BuildingCost _costPerPiece;

    public PlaceBuildingPieceUseCase(IEventBus bus, BuildingCost costPerPiece)
    {
        _bus = Guard.NotNull(bus, nameof(bus));
        _costPerPiece = Guard.NotNull(costPerPiece, nameof(costPerPiece));
    }

    /// <summary>
    /// Throws <see cref="Game.Domain.Exceptions.DomainException"/> when the placement is
    /// refused. No event is published in that case: events state what happened, never
    /// what was attempted.
    /// </summary>
    public BuildingPiece Execute(PlaceBuildingPieceCommand command, InventoryBag inventory)
    {
        Guard.NotNull(inventory, nameof(inventory));

        var piece = BuildingPiece.Build(
            command.Kind,
            command.Position,
            command.Support,
            _costPerPiece,
            inventory);

        _bus.Publish(new StructurePlaced(piece.Kind, piece.Position));
        return piece;
    }
}
