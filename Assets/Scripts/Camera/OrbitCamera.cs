using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        
        [SerializeField] private float orbitSpeed = 0.25f;
        [SerializeField] private float orbitDamping = 30.0f;
        [SerializeField] private float maxOrbitSpeed = 10.0f;
        
        private Vector2 _rotation = new(0, 0);
    
        private InputAction _orbitAction;

        private void LateUpdate()
        {
            var qt = Quaternion.Euler(_rotation.y, _rotation.x, 0);
            pivot.rotation = Quaternion.Lerp(transform.rotation, qt, Time.deltaTime * orbitDamping);
        }

        private void Start()
        {
            if (pivot is null)
            {
                Debug.LogError("Camera orbit pivot is null!");
                return;
            }
            
            _orbitAction = InputSystem.actions.FindAction("OrbitCamera");
            _orbitAction.started += OnOrbitActionPerformed;
        }

        private void OnOrbitActionPerformed(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<Vector2>();
            
            _rotation.x += Mathf.Clamp(value.x * orbitSpeed, -maxOrbitSpeed, maxOrbitSpeed);
            _rotation.x = Mathf.Repeat(_rotation.x, 360);
            
            _rotation.y -= Mathf.Clamp(value.y * orbitSpeed, -maxOrbitSpeed, maxOrbitSpeed);
            _rotation.y = Mathf.Clamp(_rotation.y, -90, 90);
        }
    }
}
