using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBehaviour : MonoBehaviour
{
    public abstract Vector3 GetVector(MovementStatus status);
}

public class MovementStatus
{
    public float currentSpeed = 0f;
    public Vector3 currentMovement = Vector3.zero;
    public Vector3 inputDirection = Vector3.zero;
}

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    public float maxSpeed;
    private Rigidbody rb;
    private MovementStatus status;
    private InputHandler inputHandler;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        status = new MovementStatus();
        inputHandler = GetComponent<InputHandler>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        status.inputDirection = inputHandler.GetMovementDirection();

        Vector3 result = Vector3.zero;
        foreach (MovementBehaviour mb in GetComponents<MovementBehaviour>())
        {
            Vector3 vector = mb.GetVector(status);
            if (vector.magnitude != 0)
            {
                Debug.DrawRay(transform.position, vector, Color.magenta);
                result += vector;
            }
        }
        Debug.Log(result);
        //Debug.DrawRay(transform.position, result, Color.green);

        float t = Time.fixedDeltaTime;

        Vector3 movement = status.currentMovement * status.currentSpeed * t + .5f * t * t * result;
        //float speedLimit = inputMovementDirection.magnitude != 0 ? maxSpeed * inputMovementDirection.magnitude : maxSpeed; //Speed is limited by the controller analogue
        status.currentSpeed = Mathf.Clamp(status.currentSpeed + Vector3.Dot(status.currentMovement, result.normalized) * result.magnitude * t, 0, maxSpeed);
        
        Debug.DrawRay(transform.position, movement, Color.yellow);

        status.currentMovement = movement.normalized;

        rb.MovePosition(transform.position + movement);
    }
}
