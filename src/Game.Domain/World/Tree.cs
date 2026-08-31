using System.Numerics;
using Game.Domain.Exceptions;

namespace Game.Domain.World;

/// <summary>
/// Rich entity: a standing tree that can be felled exactly once.
/// It knows what it yields; it does not know who fells it or what happens next.
/// Coordinates use System.Numerics.Vector3, never UnityEngine.Vector3 (Gate A).
/// </summary>
public sealed class Tree
{
    public string Id { get; }
    public Vector3 Position { get; }
    public string YieldItemId { get; }
    public int YieldAmount { get; }
    public float YieldUnitWeight { get; }
    public bool IsFelled { get; private set; }

    public Tree(string id, Vector3 position, string yieldItemId, int yieldAmount, float yieldUnitWeight)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Tree id is required.");
        if (string.IsNullOrWhiteSpace(yieldItemId))
            throw new DomainException("Tree yield item id is required.");
        if (yieldAmount <= 0)
            throw new DomainException("Tree yield amount must be greater than zero.");
        if (yieldUnitWeight < 0f)
            throw new DomainException("Tree yield unit weight cannot be negative.");

        Id = id;
        Position = position;
        YieldItemId = yieldItemId;
        YieldAmount = yieldAmount;
        YieldUnitWeight = yieldUnitWeight;
    }

    /// <summary>Fells the tree. A tree already on the ground cannot be felled again.</summary>
    public void Fell()
    {
        if (IsFelled)
            throw new DomainException($"Tree '{Id}' has already been felled.");

        IsFelled = true;
    }
}
