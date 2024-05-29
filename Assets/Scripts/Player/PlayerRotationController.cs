using UnityEngine;

public class PlayerRotationController : MonoBehaviour
{
    [SerializeField] private float baseRotationSpeed = 800f;
    private float RotationSpeed { get; set; }

    private Rigidbody rb;
    private IInputHandler inputHandler;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputHandler = GetComponentInParent<IInputHandler>();
        RotationSpeed = baseRotationSpeed;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        var lookDirection = inputHandler.GetLookDirection();
        LookRotation(lookDirection);
    }

    private void LookRotation(Vector3 lookDirection)
    {
        float angle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
        float step = RotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, angle, 0), step));
    }
    
    public void AddPercentage(float percent)
    {
        RotationSpeed += baseRotationSpeed * percent / 100;
    }
    
    public void SubPercentage(float percent)
    {
        RotationSpeed -= baseRotationSpeed * percent / 100;
    }
}
