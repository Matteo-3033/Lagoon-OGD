using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5F;
    
    private Rigidbody rb;
    private Vector3 movement;
    
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        movement = speed * new Vector3(Input.GetAxis("Horizontal"), 0F, Input.GetAxis("Vertical"));
    }
    
    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + movement);
    }
}
