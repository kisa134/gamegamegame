using Game.Domain.Events;

namespace Game.Application.Events;

/// <summary>
/// Publish/subscribe bus for domain events. Enables emergent choreography:
/// systems react to facts without direct calls. Use Cases publish; subscribers react.
/// </summary>
public interface IEventBus
{
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent;
    void Publish<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
}
