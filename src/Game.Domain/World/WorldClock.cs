using Game.Domain.Exceptions;

namespace Game.Domain.World;

/// <summary>
/// Rich entity: world time. It only ever moves forward, and it is the single place
/// that decides what counts as night. The Unity layer supplies elapsed seconds;
/// it does not get to decide what time it is.
/// </summary>
public sealed class WorldClock
{
    private const float NightFalls = 20f;
    private const float DayBreaks = 6f;

    public int Day { get; private set; }
    public float HourOfDay { get; private set; }

    public bool IsNight => HourOfDay >= NightFalls || HourOfDay < DayBreaks;

    public WorldClock(int startDay, float startHour)
    {
        if (startDay < 1)
            throw new DomainException("World starts on day 1 or later.");
        if (startHour < 0f || startHour >= 24f)
            throw new DomainException("Hour of day must be in [0, 24).");

        Day = startDay;
        HourOfDay = startHour;
    }

    /// <summary>
    /// Moves time forward. Returns true only on the tick that crosses into night,
    /// so a caller can publish that fact exactly once per day.
    /// </summary>
    public bool Advance(float hours)
    {
        if (hours < 0f)
            throw new DomainException("Time does not run backwards.");

        var wasNight = IsNight;

        HourOfDay += hours;
        while (HourOfDay >= 24f)
        {
            HourOfDay -= 24f;
            Day++;
        }

        return !wasNight && IsNight;
    }
}
