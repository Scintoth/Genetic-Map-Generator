using UnityEngine;

namespace GeneticMapGenerator.Unity.Views
{
    /// <summary>
    /// Orbits a focus point. Drifts slowly on its own; with the legacy input manager enabled, drag with
    /// the right mouse button to orbit and scroll to zoom. Focus changes glide rather than cut.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class OrbitCameraRig : MonoBehaviour
    {
        [SerializeField] private float autoRotateDegreesPerSecond = 4f;
        [SerializeField] private float dragDegreesPerPixel = 0.25f;
        [SerializeField] private float glideSharpness = 3f;
        [SerializeField, Range(10, 85)] private float pitch = 38f;

        private Vector3 _focus;
        private Vector3 _targetFocus;
        private float _distance = 300f;
        private float _targetDistance = 300f;
        private float _yaw = 30f;

        public void Frame(Vector3 focus, float distance, bool immediately = false)
        {
            _targetFocus = focus;
            _targetDistance = distance;
            if (immediately)
            {
                _focus = focus;
                _distance = distance;
            }
        }

        private void LateUpdate()
        {
            ReadInput();

            var blend = 1f - Mathf.Exp(-glideSharpness * Time.unscaledDeltaTime);
            _focus = Vector3.Lerp(_focus, _targetFocus, blend);
            _distance = Mathf.Lerp(_distance, _targetDistance, blend);

            var rotation = Quaternion.Euler(pitch, _yaw, 0);
            transform.SetPositionAndRotation(_focus - rotation * Vector3.forward * _distance, rotation);
        }

        private void ReadInput()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButton(1))
            {
                _yaw += Input.GetAxis("Mouse X") * dragDegreesPerPixel * 20f;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * dragDegreesPerPixel * 20f, 10, 85);
                return;
            }

            var scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetDistance = Mathf.Clamp(_targetDistance * (1f - scroll * 0.1f), 5f, 2000f);
            }
#endif
            _yaw += autoRotateDegreesPerSecond * Time.unscaledDeltaTime;
        }
    }
}
