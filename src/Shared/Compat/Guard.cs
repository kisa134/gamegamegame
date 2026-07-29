using System;

namespace Game.Compat;

/// <summary>
/// Argument guards that exist on every target we build for.
/// <c>ArgumentNullException.ThrowIfNull</c> is .NET 6+, and the core also compiles
/// against netstandard2.1 so Unity can consume it — see the csproj TargetFrameworks.
/// Same exception type, so behaviour and tests are unchanged.
/// </summary>
internal static class Guard
{
    /// <summary>Unconstrained on purpose: event payloads are generic over IDomainEvent, not over class.</summary>
    public static T NotNull<T>(T value, string paramName)
        => value is null ? throw new ArgumentNullException(paramName) : value;
}
