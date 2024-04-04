using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5F;

    private CustomInput input = null;
    private Rigidbody rb;

    private Vector3 movement;
    private Vector3 direction;
    private Camera camera;

    private void Awake()
    {
        input = new CustomInput();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    
    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + speed * Time.fixedDeltaTime * movement);
        rb.MoveRotation(Quaternion.AngleAxis(Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, transform.up));
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += Movement_performed;
        input.Player.Movement.canceled += Movement_canceled;

        input.Player.MousePosition.performed += MousePosition_performed;
        input.Player.View.performed += View_performed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= Movement_performed;
        input.Player.Movement.canceled -= Movement_canceled;
        
        input.Player.MousePosition.performed -= MousePosition_performed;
        input.Player.View.performed -= View_performed;
    }

    private void Movement_performed(InputAction.CallbackContext callbackContext)
    {
        Vector2 temp = callbackContext.ReadValue<Vector2>();
        movement = new Vector3(temp.x, 0, temp.y);
    }

    private void Movement_canceled(InputAction.CallbackContext callbackContext)
    {
        movement = Vector3.zero;
    }

    private void MousePosition_performed(InputAction.CallbackContext callbackContext)
    {
        Vector3 mousePos = callbackContext.ReadValue<Vector2>();
        mousePos.z = camera.transform.position.y - transform.position.y;
        Vector3 lookPosition = camera.ScreenToWorldPoint(mousePos);
        direction = (lookPosition - transform.position).normalized;
    }

    private void View_performed(InputAction.CallbackContext callbackContext)
    {
        Vector3 temp = callbackContext.ReadValue<Vector2>();
        direction = new Vector3(temp.x, 0, temp.y);
    }
}
