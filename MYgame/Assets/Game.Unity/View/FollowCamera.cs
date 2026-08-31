using UnityEngine;

namespace Game.Unity.View
{
    /// <summary>
    /// Third-person boom. Runs in LateUpdate, after the CharacterController has moved —
    /// which is the whole reason this is a script and not a parented transform.
    /// Cinemachine is not installed in this project.
    /// </summary>
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 2.2f, -4.5f);
        [SerializeField] private float _smoothing = 12f;

        private float _pitch = 12f;

        public void Initialise(Transform target)
        {
            _target = target;
        }

        public void SetPitch(float pitch)
        {
            _pitch = Mathf.Clamp(pitch, -25f, 60f);
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            var rotation = Quaternion.Euler(_pitch, _target.eulerAngles.y, 0f);
            var wanted = _target.position + rotation * _offset;

            transform.position = Vector3.Lerp(transform.position, wanted, Time.deltaTime * _smoothing);
            transform.LookAt(_target.position + Vector3.up * 1.4f);
        }
    }
}
