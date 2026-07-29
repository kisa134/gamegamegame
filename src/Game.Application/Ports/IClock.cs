namespace Game.Application.Ports;

/// <summary>
/// Port: world time. The Unity layer drives it; the core only reads it.
/// Kept free of any engine type so the core stays testable headless.
/// </summary>
public interface IClock
{
    /// <summary>Day number since the world began, starting at 1.</summary>
    int Day { get; }

    /// <summary>Hour of day in [0, 24). 6.5 means half past six in the morning.</summary>
    float HourOfDay { get; }

    bool IsNight { get; }
}
