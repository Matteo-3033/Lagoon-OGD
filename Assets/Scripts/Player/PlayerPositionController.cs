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

    public IInputHanlder inputHanlder;
    private Rigidbody rb;

    private Vector3 currentSpeed;
    private Vector3 inputMovementDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputHanlder = GetComponentInParent<IInputHanlder>();
    }

    private void FixedUpdate()
    {
        inputMovementDirection = inputHanlder.GetMovementDirection();

        if (acceleratedMovement)
        {
            AcceleratedMovement();
        }
        else
        {
            InstantMovement();
        }
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
}
