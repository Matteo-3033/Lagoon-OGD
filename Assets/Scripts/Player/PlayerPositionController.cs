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
    private Animator _animator;
    [SyncVar] private float speedValue;
    private bool isLocal = true;
    private static readonly int SpeedParam = Animator.StringToHash("speed");


    private void Start()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        foreach (Animator a in animators)
        {
            foreach (AnimatorControllerParameter parameter in a.parameters)
            {
                if (parameter.nameHash != SpeedParam) continue;

                _animator = a;
                break;
            }
        }

        var player = GetComponent<Player>();

#if !UNITY_EDITOR
        if (!player.isLocalPlayer)
        {
            isLocal = false;
            return;
        }
#endif

        rb = GetComponent<Rigidbody>();
        inputHandler = player.InputHandler;
    }

    private void FixedUpdate()
    {
        if (isServer) return;

        if (isLocal)
        {
            speedValue = currentSpeed.magnitude;
            CmdUpdateAnimation(speedValue);
        }
        else
        {
            // Debug.Log("Speed value received: " + speedValue);
        }

        _animator?.SetFloat(SpeedParam, speedValue);

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
    }

    [Command(requiresAuthority = false)]
    private void CmdUpdateAnimation(float value)
    {
        speedValue = value;
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

    private void OnDisable()
    {
        if (rb)
        {
            rb.velocity = Vector3.zero;
        }
    }
}