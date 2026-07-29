using Game.Application.Events;
using Game.Compat;
using Game.Domain.Events;
using Game.Domain.World;

namespace Game.Application.UseCases;

/// <summary>
/// Drives world time forward and announces nightfall. The clock decides when night is;
/// this publishes the fact so lights, sounds and NPCs can react without knowing about
/// each other. Events are published here, never by the entity (CLAUDE.md section 3).
/// </summary>
public sealed class AdvanceWorldTimeUseCase
{
    private readonly WorldClock _clock;
    private readonly IEventBus _bus;

    public AdvanceWorldTimeUseCase(WorldClock clock, IEventBus bus)
    {
        _clock = Guard.NotNull(clock, nameof(clock));
        _bus = Guard.NotNull(bus, nameof(bus));
    }

    /// <summary>Advances by the given number of game hours. Returns true if night fell on this tick.</summary>
    public bool Execute(float hours)
    {
        if (!_clock.Advance(hours))
        {
            return false;
        }

        _bus.Publish(new NightFell(_clock.Day));
        return true;
    }
}
