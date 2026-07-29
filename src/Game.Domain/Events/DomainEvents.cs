using System.Numerics;

namespace Game.Domain.Events;

/// <summary>Marker for a domain event. Events are published by Use Cases, never by entities.</summary>
public interface IDomainEvent
{
}

public sealed record TreeChopped(string TreeId, string ItemId, int Amount) : IDomainEvent;
public sealed record StructurePlaced(string Kind, Vector3 Position) : IDomainEvent;
public sealed record CharacterDamaged(string Name, int Amount, int RemainingHealth) : IDomainEvent;
public sealed record EntityKilled(string Name) : IDomainEvent;
public sealed record DialogueStarted(string NpcName) : IDomainEvent;
public sealed record NightFell(int Day) : IDomainEvent;
public sealed record ArtifactSensed(string NpcName) : IDomainEvent;
