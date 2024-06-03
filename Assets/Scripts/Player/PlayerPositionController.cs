using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPositionController : MonoBehaviour
{
    [SerializeField] private float baseMaxSpeed = 10F;
    [Space] [SerializeField] private bool acceleratedMovement = true;
    [Space] [SerializeField] private bool forceMovementPosition = false;
    [SerializeField] private float acceleration = 50F;
    [SerializeField] private float deacceleration = 25F;

    private float factor = 1F;

    private float MaxSpeed => baseMaxSpeed * factor;
    
    private IInputHandler inputHandler;
    private Rigidbody rb;

    private Vector3 currentSpeed;
    private List<Vector3> additionalVectors = new();

    private void Start()
    {
        var player = GetComponent<Player>();

#if !UNITY_EDITOR
        if (!player.isLocalPlayer)
            return;
#endif

        rb = GetComponent<Rigidbody>();
        inputHandler = player.InputHandler;
    }

    private void FixedUpdate()
    {
        if (inputHandler == null)
            return;
        
        Vector3 inputDirection = inputHandler.GetMovementDirection();

        if (acceleratedMovement)
        {
            AcceleratedMovement(inputDirection);
        }
        else
        {
            InstantMovement(inputDirection);
        }
    }

    private void AcceleratedMovement(Vector3 inputDirection)
    {
        float t = Time.fixedDeltaTime;
        Vector3 accelerationComponent = inputDirection * acceleration;
        if (inputDirection.magnitude <= .1f)
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

        foreach (Vector3 v in additionalVectors)
        {
            accelerationComponent += v;
        }

        float speedLimit = inputDirection.magnitude != 0 ? MaxSpeed * inputDirection.magnitude : MaxSpeed; //Speed is limited by the controller analogue
        Vector3 movement = currentSpeed * t + .5f * t * t * accelerationComponent;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed + t * accelerationComponent, speedLimit);

        Debug.DrawRay(transform.position, currentSpeed, Color.cyan);

#if UNITY_EDITOR
        if (forceMovementPosition)
        {
            rb.MovePosition(transform.position + movement);
        }
        else
        {
            rb.velocity = currentSpeed;
        }
#else
        rb.velocity = currentSpeed;
#endif
        //Debug.Log("Speed: " + movement.magnitude / t + " m/s");
    }

    private void InstantMovement(Vector3 inputDirection)
    {
        float t = Time.fixedDeltaTime;

        float speedLimit =
            inputDirection.magnitude != 0
                ? MaxSpeed * inputDirection.magnitude
                : MaxSpeed; //Speed is limited by the controller analogue
        Vector3 movement = inputDirection * (t * speedLimit);

        Debug.DrawRay(transform.position, movement * 10, Color.green);
        Debug.DrawRay(transform.position, currentSpeed, Color.cyan);

        rb.MovePosition(transform.position + movement);
        //Debug.Log("Speed: " + movement.magnitude / t + " m/s");
    }

    public void AddVector(Vector3 vector)
    {
        additionalVectors.Add(vector);
    }

    public void AddFactor(float f)
    {
        factor *= f;
    }
}
