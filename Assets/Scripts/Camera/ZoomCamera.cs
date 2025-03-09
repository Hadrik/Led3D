using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
    public class ZoomCamera : MonoBehaviour
    {
        [SerializeField] private float distance = 2.0f;
        [SerializeField] private float maxDistance = 5.0f;
        [SerializeField] private float minDistance = 1.0f;
        
        [SerializeField] private float zoomSpeed = 0.3f;
        [SerializeField] private float zoomDamping = 20.0f;
        
        private InputAction _zoomAction;
        
        private void LateUpdate()
        {
            var dist = Mathf.Lerp(transform.localPosition.z, -distance, Time.deltaTime * zoomDamping);
            transform.localPosition = new Vector3(0, 0, dist);
        }
        
        private void Start()
        {
            _zoomAction = InputSystem.actions.FindAction("ZoomCamera");
            _zoomAction.performed += OnZoomActionPerformed;
        }
        
        private void OnZoomActionPerformed(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<float>();
            
            distance = Mathf.Clamp(distance - value * zoomSpeed, minDistance, maxDistance);
        }
    }
}
