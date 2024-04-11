using Mirror;
using UnityEngine;

public class PlayerRotationController : NetworkBehaviour
{
    [SerializeField] private float rotationSpeed = 800f;

    private Rigidbody rb;
    private IInputHanlder inputHandler;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputHandler = GetComponentInParent<IInputHanlder>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (isServer)
            return;
        
        var lookDirection = inputHandler.GetLookDirection();
        LookRotation(lookDirection);
    }

    private void LookRotation(Vector3 lookDirection)
    {
        float angle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
        float step = rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, angle, 0), step));
    }
}
