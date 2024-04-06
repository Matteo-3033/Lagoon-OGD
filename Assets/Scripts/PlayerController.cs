using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 10F;
    [Space]
    [SerializeField] private bool acceleratedMovement = true;
    [SerializeField] private float acceleration = 50F;
    [SerializeField] private float deacceleration = 25F;
    [Space]
    [SerializeField] private float rotationSpeed = 5F;

    private CustomInput input = null;
    private Rigidbody rb;
    private Camera mainCamera;

    private Vector3 currentSpeed;
    private Vector3 inputMovementDirection;
    private Vector3 lookDirection;

    private void Awake()
    {
        input = new CustomInput();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if (acceleratedMovement)
        {
            AcceleratedMovement();
        }
        else
        {
            InstantMovement();
        }

        LookRotation();
    }

    private void LookRotation()
    {
        float angle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
        float step = rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, angle, 0), step));
    }

    private void AcceleratedMovement()
    {
        float t = Time.fixedDeltaTime;
        Vector3 accelerationComponent = inputMovementDirection * acceleration;
        if (inputMovementDirection.magnitude <= .1f)
        {
            if (currentSpeed.magnitude <= deacceleration * t)
            {
                currentSpeed = Vector3.zero;
            }
            else
            {
                accelerationComponent -= currentSpeed.normalized * deacceleration;
            }
        }

        float speedLimit = inputMovementDirection.magnitude != 0 ? maxSpeed * inputMovementDirection.magnitude : maxSpeed; //Speed is limited by the controller analogue
        Vector3 movement = currentSpeed * t + .5f * t * t * accelerationComponent;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed + t * accelerationComponent, speedLimit);

        Debug.DrawRay(transform.position, currentSpeed, Color.cyan);

        rb.MovePosition(transform.position + movement);
        Debug.Log("Speed: " + movement.magnitude / t + " m/s");
    }

    private void InstantMovement()
    {
        float t = Time.fixedDeltaTime;

        float speedLimit = inputMovementDirection.magnitude != 0 ? maxSpeed * inputMovementDirection.magnitude : maxSpeed; //Speed is limited by the controller analogue
        Vector3 movement = inputMovementDirection * t * speedLimit;

        Debug.DrawRay(transform.position, movement * 10, Color.green);
        Debug.DrawRay(transform.position, currentSpeed, Color.cyan);

        rb.MovePosition(transform.position + movement);
        Debug.Log("Speed: " + movement.magnitude / t + " m/s");
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
        inputMovementDirection = new Vector3(temp.x, 0, temp.y);
    }

    private void Movement_canceled(InputAction.CallbackContext callbackContext)
    {
        inputMovementDirection = Vector3.zero;
    }

    private void MousePosition_performed(InputAction.CallbackContext callbackContext)
    {
        Vector3 mousePos = callbackContext.ReadValue<Vector2>();
        mousePos.z = mainCamera.transform.position.y - transform.position.y;
        Vector3 lookPosition = mainCamera.ScreenToWorldPoint(mousePos);
        lookDirection = (lookPosition - transform.position).normalized;
    }

    private void View_performed(InputAction.CallbackContext callbackContext)
    {
        Vector3 temp = callbackContext.ReadValue<Vector2>();
        lookDirection = new Vector3(temp.x, 0, temp.y);
    }
}
