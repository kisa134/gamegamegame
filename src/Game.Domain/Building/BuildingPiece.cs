using System.Numerics;
using Game.Domain.Exceptions;

namespace Game.Domain.Building;

/// <summary>
/// Rich entity: a placed building piece. A piece cannot exist in the air —
/// construction requires support (ground or an existing piece).
/// Coordinates use System.Numerics.Vector3, never UnityEngine.Vector3 (Gate A).
/// </summary>
public sealed class BuildingPiece
{
    public string Kind { get; }
    public Vector3 Position { get; }
    public SupportKind Support { get; }

    public BuildingPiece(string kind, Vector3 position, SupportKind support)
    {
        if (string.IsNullOrWhiteSpace(kind))
            throw new DomainException("Building piece kind is required.");
        if (support == SupportKind.None)
            throw new DomainException("Cannot place a building piece in the air: support required.");

        Kind = kind;
        Position = position;
        Support = support;
    }
}
