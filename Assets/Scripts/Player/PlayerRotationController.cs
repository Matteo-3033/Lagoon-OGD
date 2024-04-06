using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotationController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 800f;

    private Rigidbody rb;
    private IInputHanlder inputHanlder;

    private Vector3 lookDirection;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inputHanlder = GetComponentInParent<IInputHanlder>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lookDirection = inputHanlder.GetLookDirection();
        LookRotation();
    }

    private void LookRotation()
    {
        float angle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
        float step = rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, angle, 0), step));
    }
}
