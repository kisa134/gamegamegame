using Game.Application.Ports;
using Game.Application.UseCases;
using Game.Domain.World;
using UnityEngine;

namespace Game.Unity.Adapters
{
    /// <summary>
    /// Feeds elapsed real seconds into world time. It converts and delegates; it does not
    /// decide what time it is or when night falls — the WorldClock entity owns that, and
    /// the use case announces it. No game rule lives in this Update.
    /// </summary>
    public sealed class UnityClock : MonoBehaviour, IClock
    {
        [Tooltip("Real seconds per in-game day. 120 keeps a playtest short enough to see nightfall.")]
        [SerializeField] private float _secondsPerDay = 120f;

        private WorldClock _clock;
        private AdvanceWorldTimeUseCase _advance;

        public int Day => _clock?.Day ?? 1;
        public float HourOfDay => _clock?.HourOfDay ?? 0f;
        public bool IsNight => _clock != null && _clock.IsNight;

        public void Initialise(WorldClock clock, AdvanceWorldTimeUseCase advance)
        {
            _clock = clock;
            _advance = advance;
        }

        private void Update()
        {
            if (_advance == null || _secondsPerDay <= 0f)
            {
                return;
            }

            _advance.Execute(Time.deltaTime / _secondsPerDay * 24f);
        }
    }
}
