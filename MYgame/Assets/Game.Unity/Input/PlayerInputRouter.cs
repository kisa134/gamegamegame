using Game.Application.UseCases;
using Game.Domain.Building;
using Game.Domain.Exceptions;
using Game.Unity.Adapters;
using Game.Unity.View;
using UnityEngine;
using UnityEngine.InputSystem;
using InventoryBag = Game.Domain.Inventory.Inventory;

namespace Game.Unity.Input
{
    /// <summary>
    /// Turns keys and clicks into intentions and hands them to use cases. Contains no game
    /// rule: whether a tree can be chopped or a wall afforded is decided by the core.
    /// This project is configured for the new Input System only, so UnityEngine.Input is
    /// never used here — it would compile and then throw at runtime.
    /// </summary>
    public sealed class PlayerInputRouter : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5.5f;
        [SerializeField] private float _turnSpeed = 200f;
        [SerializeField] private float _reach = 3.5f;
        [SerializeField] private float _gravity = -18f;

        private CharacterController _controller;
        private FollowCamera _camera;
        private ChopTreeUseCase _chop;
        private PlaceBuildingPieceUseCase _place;
        private InventoryBag _inventory;
        private Animator _animator;

        private float _verticalSpeed;
        private float _pitch = 12f;

        /// <summary>Last thing the world said back to the player. Shown in the readout.</summary>
        public string LastMessage { get; private set; } = "WASD to walk, mouse to look, LMB chop, RMB build";

        public void Initialise(
            CharacterController controller,
            FollowCamera followCamera,
            ChopTreeUseCase chop,
            PlaceBuildingPieceUseCase place,
            InventoryBag inventory,
            Animator animator)
        {
            _controller = controller;
            _camera = followCamera;
            _chop = chop;
            _place = place;
            _inventory = inventory;
            _animator = animator;
        }

        private void Update()
        {
            if (_controller == null)
            {
                return;
            }

            Look();
            Move();
            Act();
        }

        private void Look()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            var delta = mouse.delta.ReadValue();
            transform.Rotate(Vector3.up, delta.x * _turnSpeed * Time.deltaTime * 0.02f);

            _pitch = Mathf.Clamp(_pitch - delta.y * 0.08f, -25f, 60f);
            if (_camera != null)
            {
                _camera.SetPitch(_pitch);
            }
        }

        private void Move()
        {
            var keyboard = Keyboard.current;
            var forward = 0f;
            var strafe = 0f;

            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) forward += 1f;
                if (keyboard.sKey.isPressed) forward -= 1f;
                if (keyboard.dKey.isPressed) strafe += 1f;
                if (keyboard.aKey.isPressed) strafe -= 1f;
            }

            var wish = (transform.forward * forward + transform.right * strafe);
            if (wish.sqrMagnitude > 1f)
            {
                wish.Normalize();
            }

            _verticalSpeed = _controller.isGrounded
                ? -1f
                : _verticalSpeed + _gravity * Time.deltaTime;

            var motion = wish * _moveSpeed + Vector3.up * _verticalSpeed;
            _controller.Move(motion * Time.deltaTime);

            if (_animator != null)
            {
                _animator.SetFloat("Speed", wish.magnitude * _moveSpeed);
            }
        }

        private void Act()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            // Aim is encoded into the origin: the core's world query takes a point and a
            // reach, not a direction, so we ask about the point just in front of the player.
            var aim = NumericsBridge.ToNumerics(transform.position + transform.forward * 1.2f + Vector3.up * 0.8f);

            if (mouse.leftButton.wasPressedThisFrame)
            {
                try
                {
                    var chopped = _chop.Execute(new ChopTreeCommand(aim, _reach), _inventory);
                    LastMessage = chopped ? "Chopped a tree." : "Nothing in reach.";
                }
                catch (DomainException ex)
                {
                    LastMessage = ex.Message;
                }
            }

            if (mouse.rightButton.wasPressedThisFrame)
            {
                var spot = transform.position + transform.forward * 2f;
                try
                {
                    _place.Execute(
                        new PlaceBuildingPieceCommand(
                            "wall",
                            NumericsBridge.ToNumerics(spot),
                            SupportKind.Ground),
                        _inventory);
                    LastMessage = "Placed a piece.";
                }
                catch (DomainException ex)
                {
                    LastMessage = ex.Message;
                }
            }
        }
    }
}
