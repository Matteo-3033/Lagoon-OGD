using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputHanlder
{
    Vector3 GetMovementDirection();
    Vector3 GetLookDirection();
}

public class InputHandler : MonoBehaviour, IInputHanlder
{

    private CustomInput input = null;

    private Vector3 inputMovementDirection;
    private Vector3 lookDirection;
    private Vector3 lookPosition;
    private bool mousePerformed;

    public delegate void Move(Vector3 inputDirection);

    private void Awake()
    {
        input = new CustomInput();
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
        mousePos.z = Camera.main.transform.position.y / Mathf.Cos((90 - Camera.main.fieldOfView) / 2 * Mathf.Deg2Rad);
        lookPosition = Camera.main.ScreenToWorldPoint(mousePos);
        lookPosition = Quaternion.AngleAxis(90 - Camera.main.transform.rotation.eulerAngles.x, Camera.main.transform.right) * lookPosition;
        //lookPosition.y = transform.position.y;
        Debug.DrawLine(Camera.main.transform.position, lookPosition, Color.magenta, 3);
        Debug.DrawLine(transform.position, transform.position + Camera.main.transform.right, Color.cyan);
        mousePerformed = true;
    }

    private void View_performed(InputAction.CallbackContext callbackContext)
    {
        Vector3 temp = callbackContext.ReadValue<Vector2>();
        lookDirection = new Vector3(temp.x, 0, temp.y);
        mousePerformed = false;
    }

    public Vector3 GetMovementDirection()
    {
        return inputMovementDirection;
    }

    public Vector3 GetLookDirection()
    {
        if (mousePerformed)
        {
            lookDirection = (lookPosition - transform.position).normalized;
            Debug.DrawRay(transform.position, lookDirection * (lookPosition - transform.position).magnitude, Color.green);
        }
        return lookDirection;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Camera.main.transform.position, Camera.main.transform.position.y / Mathf.Cos((90 - Camera.main.fieldOfView) / 2 * Mathf.Deg2Rad));
    }
}
