using System.Numerics;
using Game.Domain.World;

namespace Game.Application.Ports;

/// <summary>
/// Port: the core asks the world what is nearby. The Unity layer implements it.
/// Declared in terms of domain types only — a port that leaks a Unity type is not a port.
/// </summary>
public interface ITreeRegistry
{
    /// <summary>
    /// Returns the nearest tree still standing within <paramref name="reach"/> of
    /// <paramref name="origin"/>, or null if there is none. Felled trees are never returned,
    /// so a caller cannot be handed a stump to chop.
    /// </summary>
    Tree? FindStandingTreeWithinReach(Vector3 origin, float reach);
}
