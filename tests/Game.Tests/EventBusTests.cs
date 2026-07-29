using Game.Application.Events;
using Game.Domain.Events;
using Xunit;

namespace Game.Tests;

public sealed class EventBusTests
{
    [Fact]
    public void Publish_InvokesMatchingSubscriber()
    {
        var bus = new InMemoryEventBus();
        CharacterDamaged? received = null;
        bus.Subscribe<CharacterDamaged>(e => received = e);

        bus.Publish(new CharacterDamaged("Alaric", 30, 70));

        Assert.NotNull(received);
        Assert.Equal(70, received!.RemainingHealth);
    }

    [Fact]
    public void Publish_DoesNotInvokeOtherEventSubscribers()
    {
        var bus = new InMemoryEventBus();
        var killedCount = 0;
        bus.Subscribe<EntityKilled>(_ => killedCount++);

        bus.Publish(new CharacterDamaged("Alaric", 1, 1));

        Assert.Equal(0, killedCount);
    }

    [Fact]
    public void Publish_InvokesAllSubscribersOfSameEvent()
    {
        var bus = new InMemoryEventBus();
        int a = 0, b = 0;
        bus.Subscribe<NightFell>(_ => a++);
        bus.Subscribe<NightFell>(_ => b++);

        bus.Publish(new NightFell(1));

        Assert.Equal(1, a);
        Assert.Equal(1, b);
    }

    [Fact]
    public void Subscribe_NullHandler_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new InMemoryEventBus().Subscribe<NightFell>(null!));
}
