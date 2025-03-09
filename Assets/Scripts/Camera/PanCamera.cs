using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
    public class PanCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        
        [SerializeField] private float panDamping = 20.0f;
        [SerializeField] private float panSpeed = 0.01f;
        
        private Vector3 _pan = new(0, 0, 0);
        
        private InputAction _panAction;

        private void LateUpdate()
        {
            target.position = Vector3.Lerp(target.position, _pan, Time.deltaTime * panDamping);
        }
        
        private void Start()
        {
            if (target is null)
            {
                Debug.LogError("Camera pan target is null!");
                return;
            }
            
            _panAction = InputSystem.actions.FindAction("PanCamera");
            _panAction.performed += OnPanActionPerformed;
        }

        private void OnPanActionPerformed(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<Vector2>();
            
            _pan -= transform.TransformVector(value * panSpeed);
        }
    }
}
