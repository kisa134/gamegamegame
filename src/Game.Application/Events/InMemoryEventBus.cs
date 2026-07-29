using Game.Domain.Events;

namespace Game.Application.Events;

/// <summary>Synchronous in-memory event bus. Deterministic and headless-testable.</summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (!_handlers.TryGetValue(typeof(TEvent), out var list))
        {
            list = new List<Delegate>();
            _handlers[typeof(TEvent)] = list;
        }

        list.Add(handler);
    }

    public void Publish<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (_handlers.TryGetValue(typeof(TEvent), out var list))
        {
            foreach (var handler in list)
            {
                ((Action<TEvent>)handler).Invoke(domainEvent);
            }
        }
    }
}
