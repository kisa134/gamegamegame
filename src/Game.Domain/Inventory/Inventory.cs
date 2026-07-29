using Game.Domain.Exceptions;

namespace Game.Domain.Inventory;

/// <summary>
/// Rich entity: resource bag with weight capacity.
/// Quantities never go negative; total weight never exceeds capacity.
/// Fields are private; mutations go only through methods that enforce invariants.
/// </summary>
public sealed class Inventory
{
    private readonly Dictionary<string, Stack> _stacks = new(StringComparer.Ordinal);
    private readonly float _maxWeight;
    private float _currentWeight;

    public Inventory(float maxWeight)
    {
        if (maxWeight <= 0f)
        {
            throw new DomainException("Inventory max weight must be greater than zero.");
        }

        _maxWeight = maxWeight;
    }

    public float MaxWeight => _maxWeight;

    public float CurrentWeight => _currentWeight;

    public float RemainingCapacity => _maxWeight - _currentWeight;

    public int GetQuantity(string itemId)
    {
        EnsureItemId(itemId);
        return _stacks.TryGetValue(itemId, out var stack) ? stack.Quantity : 0;
    }

    /// <summary>
    /// Adds items if capacity allows. Same item id must keep the same unit weight.
    /// </summary>
    public void AddItem(string itemId, int quantity, float unitWeight)
    {
        EnsureItemId(itemId);

        if (quantity <= 0)
        {
            throw new DomainException("Quantity to add must be greater than zero.");
        }

        if (unitWeight < 0f)
        {
            throw new DomainException("Unit weight cannot be negative.");
        }

        if (_stacks.TryGetValue(itemId, out var existing)
            && !AlmostEqual(existing.UnitWeight, unitWeight))
        {
            throw new DomainException(
                $"Item '{itemId}' already stored with unit weight {existing.UnitWeight}; cannot add with {unitWeight}.");
        }

        var addedWeight = unitWeight * quantity;
        if (_currentWeight + addedWeight > _maxWeight)
        {
            throw new DomainException(
                $"Cannot add {quantity} of '{itemId}': would exceed max weight {_maxWeight} (current {_currentWeight}, added {addedWeight}).");
        }

        var newQuantity = (existing?.Quantity ?? 0) + quantity;
        _stacks[itemId] = new Stack(newQuantity, unitWeight);
        _currentWeight += addedWeight;
    }

    /// <summary>
    /// Removes items. Fails if there are not enough of that item.
    /// </summary>
    public void RemoveItem(string itemId, int quantity)
    {
        EnsureItemId(itemId);

        if (quantity <= 0)
        {
            throw new DomainException("Quantity to remove must be greater than zero.");
        }

        if (!_stacks.TryGetValue(itemId, out var existing) || existing.Quantity < quantity)
        {
            var available = existing?.Quantity ?? 0;
            throw new DomainException(
                $"Cannot remove {quantity} of '{itemId}': only {available} available.");
        }

        var remaining = existing.Quantity - quantity;
        var removedWeight = existing.UnitWeight * quantity;

        if (remaining == 0)
        {
            _stacks.Remove(itemId);
        }
        else
        {
            _stacks[itemId] = existing with { Quantity = remaining };
        }

        _currentWeight -= removedWeight;
        if (_currentWeight < 0f)
        {
            _currentWeight = 0f;
        }
    }

    private static void EnsureItemId(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new DomainException("Item id is required.");
        }
    }

    private static bool AlmostEqual(float a, float b) => Math.Abs(a - b) < 0.0001f;

    private sealed record Stack(int Quantity, float UnitWeight);
}
