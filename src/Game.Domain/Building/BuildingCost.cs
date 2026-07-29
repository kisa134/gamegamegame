using Game.Domain.Exceptions;

namespace Game.Domain.Building;

/// <summary>
/// Value object: the resources a building piece requires to be placed.
/// <see cref="Free"/> exists so that "costs nothing" is a stated choice
/// rather than a forgotten argument.
/// </summary>
public sealed class BuildingCost
{
    private readonly Dictionary<string, int> _requirements;

    private BuildingCost(Dictionary<string, int> requirements) => _requirements = requirements;

    public static BuildingCost Free { get; } = new(new Dictionary<string, int>(StringComparer.Ordinal));

    public static BuildingCost Of(params (string ItemId, int Quantity)[] requirements)
    {
        ArgumentNullException.ThrowIfNull(requirements);

        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var (itemId, quantity) in requirements)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                throw new DomainException("Building cost item id is required.");
            if (quantity <= 0)
                throw new DomainException($"Building cost for '{itemId}' must be greater than zero.");
            if (!map.TryAdd(itemId, quantity))
                throw new DomainException($"Building cost lists '{itemId}' more than once.");
        }

        return new BuildingCost(map);
    }

    public bool IsFree => _requirements.Count == 0;

    public IReadOnlyDictionary<string, int> Requirements => _requirements;
}
