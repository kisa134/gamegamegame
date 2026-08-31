using Game.Application.Events;
using Game.Application.UseCases;
using Game.Domain.Building;
using Game.Domain.Events;
using Game.Domain.World;
using Game.Unity.Adapters;
using Game.Unity.Input;
using Game.Unity.Presenters;
using Game.Unity.View;
using UnityEngine;
using InventoryBag = Game.Domain.Inventory.Inventory;
using Tree = Game.Domain.World.Tree;

namespace Game.Unity.Composition
{
    /// <summary>
    /// The single place that constructs the core and joins it to the scene. Nothing else
    /// calls `new` on a domain or application type — which is what keeps the seam between
    /// the game and the engine one file wide.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private AssetLibrary _library;
        [SerializeField] private UnityClock _clock;
        [SerializeField] private SkyPresenter _sky;
        [SerializeField] private PlayerInputRouter _player;
        [SerializeField] private FollowCamera _followCamera;
        [SerializeField] private Light _sun;

        private InMemoryEventBus _bus;
        private InventoryBag _inventory;
        private SceneTreeRegistry _trees;

        private void Awake()
        {
            _bus = new InMemoryEventBus();
            _inventory = new InventoryBag(200f);
            _trees = new SceneTreeRegistry();

            BindScenery();

            // Start late in the day so a playtest actually reaches nightfall.
            var worldClock = new WorldClock(1, 17.5f);
            var advanceTime = new AdvanceWorldTimeUseCase(worldClock, _bus);
            if (_clock != null)
            {
                _clock.Initialise(worldClock, advanceTime);
            }

            var chop = new ChopTreeUseCase(_trees, _bus);
            var place = new PlaceBuildingPieceUseCase(_bus, BuildingCost.Of(("wood", 2)));

            SubscribePresenters();

            if (_player != null)
            {
                _player.Initialise(
                    _player.GetComponent<CharacterController>(),
                    _followCamera,
                    chop,
                    place,
                    _inventory,
                    _player.GetComponentInChildren<Animator>());
            }

            if (_sky != null && _clock != null)
            {
                _sky.Initialise(_bus, _clock, _sun);
            }

            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>Every tree the scene placed becomes a domain entity the core can reason about.</summary>
        private void BindScenery()
        {
            var views = Object.FindObjectsByType<TreeView>(FindObjectsSortMode.None);
            for (var i = 0; i < views.Length; i++)
            {
                var view = views[i];
                var position = NumericsBridge.ToNumerics(view.transform.position);
                view.Bind(new Tree($"tree-{i:D3}", position, "wood", 3, 1f));
                _trees.Register(view);
            }

            Debug.Log($"[Bootstrap] Bound {views.Length} trees.");
        }

        private void SubscribePresenters()
        {
            _bus.Subscribe<TreeChopped>(e =>
            {
                var go = _trees.FindGameObject(e.TreeId);
                if (go != null)
                {
                    Destroy(go);
                }
            });

            _bus.Subscribe<StructurePlaced>(e =>
            {
                if (_library != null)
                {
                    _library.Spawn("building.piece", NumericsBridge.ToUnity(e.Position), Quaternion.identity);
                }
            });
        }

        private void OnGUI()
        {
            if (_inventory == null)
            {
                return;
            }

            var style = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            GUI.Label(new Rect(16, 12, 640, 26),
                $"Wood: {_inventory.GetQuantity("wood")}", style);
            GUI.Label(new Rect(16, 38, 640, 26),
                $"Day {_clock?.Day ?? 1}   {_clock?.HourOfDay ?? 0f:00.0}h   {(_clock != null && _clock.IsNight ? "night" : "day")}", style);
            GUI.Label(new Rect(16, 64, 900, 26),
                _player != null ? _player.LastMessage : string.Empty, style);

            // Crosshair.
            GUI.Label(new Rect(Screen.width / 2f - 4, Screen.height / 2f - 10, 20, 20), "+", style);
        }
    }
}
