using Game.Application.Events;
using Game.Application.Ports;
using Game.Domain.Events;
using UnityEngine;

namespace Game.Unity.Presenters
{
    /// <summary>
    /// Reacts to the world getting dark. The discrete transition is driven by the NightFell
    /// event — the bus doing real work, and the same hook a future NPC's "head for the fire"
    /// reaction will subscribe to. The continuous sun angle is merely polled from the clock.
    /// </summary>
    public sealed class SkyPresenter : MonoBehaviour
    {
        [SerializeField] private Light _sun;

        private static readonly Color DaySun = new Color(1f, 0.77f, 0.48f);
        private static readonly Color DayFog = new Color(1f, 0.83f, 0.62f);
        private static readonly Color NightSun = new Color(0.55f, 0.62f, 0.95f);
        private static readonly Color NightFog = new Color(0.14f, 0.16f, 0.26f);

        private IClock _clock;
        private float _blend;
        private float _target;

        public void Initialise(IEventBus bus, IClock clock, Light sun)
        {
            _clock = clock;
            _sun = sun;
            bus.Subscribe<NightFell>(_ => _target = 1f);
        }

        private void Update()
        {
            if (_sun == null || _clock == null)
            {
                return;
            }

            // Dawn needs no event: when the clock says it is day again, ease back.
            if (!_clock.IsNight)
            {
                _target = 0f;
            }

            _blend = Mathf.MoveTowards(_blend, _target, Time.deltaTime / 4f);

            _sun.color = Color.Lerp(DaySun, NightSun, _blend);
            _sun.intensity = Mathf.Lerp(1.2f, 0.15f, _blend);

            RenderSettings.fogColor = Color.Lerp(DayFog, NightFog, _blend);
            RenderSettings.fogEndDistance = Mathf.Lerp(500f, 180f, _blend);
            RenderSettings.ambientSkyColor = Color.Lerp(
                new Color(0.62f, 0.60f, 0.78f), new Color(0.16f, 0.17f, 0.26f), _blend);

            // Sun angle tracks the hour continuously; only the palette is event-driven.
            var pitch = Mathf.Lerp(18f, -8f, _blend);
            _sun.transform.rotation = Quaternion.Euler(pitch, -35f, 0f);
        }
    }
}
