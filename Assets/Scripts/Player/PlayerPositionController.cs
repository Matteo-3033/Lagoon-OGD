using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPositionController : NetworkBehaviour
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

    public float SpeedValue { get; private set; }


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
        if (isServer) return;

        if (isLocalPlayer) SpeedValue = currentSpeed.magnitude;
        if (inputHandler == null) return;

        Vector3 inputDirection = inputHandler.GetMovementDirection();

        if (acceleratedMovement)
        {
            AcceleratedMovement(inputDirection);
        }
        else
        {
            InstantMovement(inputDirection);
        }

        if (isLocalPlayer) CommandUpdateSpeed(currentSpeed);
    }

    [Command(requiresAuthority = false)]
    private void CommandUpdateSpeed(Vector3 speed)
    {
        currentSpeed = speed;
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

        float speedLimit =
            inputDirection.magnitude != 0
                ? MaxSpeed * inputDirection.magnitude
                : MaxSpeed; //Speed is limited by the controller analogue
        currentSpeed = Vector3.ClampMagnitude(currentSpeed + t * accelerationComponent, speedLimit);

        Debug.DrawRay(transform.position, currentSpeed, Color.cyan);

#if UNITY_EDITOR
        if (forceMovementPosition)
        {
            rb.MovePosition(transform.position + (currentSpeed * t + .5f * t * t * accelerationComponent));
        }
        else
        {
            rb.velocity = currentSpeed;
        }
#else
        rb.velocity = currentSpeed;
#endif
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
    }

    public void AddVector(Vector3 vector)
    {
        additionalVectors.Add(vector);
    }

    public void AddFactor(float f)
    {
        factor *= f;
    }

    public Vector3 GetCurrentSpeed()
    {
        return currentSpeed;
    }

    private void OnDisable()
    {
        if (!rb) return;

        rb.velocity = Vector3.zero;
        SpeedValue = 0F;
    }
}