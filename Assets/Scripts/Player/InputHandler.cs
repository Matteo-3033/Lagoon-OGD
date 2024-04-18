using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputHanlder
{
    Vector3 GetMovementDirection();
    Vector3 GetLookDirection();

    public event EventHandler<EventArgs> OnInteract;
    public event EventHandler<EventArgs> OnPlaceTrap;
    public event EventHandler<int> OnSelectTrap;
}

public class InputHandler : MonoBehaviour, IInputHanlder
{
    public LayerMask groundLayerMask;

    private CustomInput input = null;

    private Vector3 inputMovementDirection;
    private Vector3 lookDirection;
    private Vector3 mousePosition;
    private bool mousePerformed;

    public delegate void Move(Vector3 inputDirection);
    
    public event EventHandler<EventArgs> OnInteract;
    public event EventHandler<EventArgs> OnPlaceTrap;
    public event EventHandler<int> OnSelectTrap;
    

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

        input.Player.Interaction.performed += Interaction_performed;
        
        input.Player.PlaceTrap.performed += PlaceTrap_performed;
        input.Player.SelectTrap.performed += SelectTrap_performed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= Movement_performed;
        input.Player.Movement.canceled -= Movement_canceled;

        input.Player.MousePosition.performed -= MousePosition_performed;
        input.Player.View.performed -= View_performed;

        input.Player.Interaction.performed -= Interaction_performed;
        
        input.Player.PlaceTrap.performed -= PlaceTrap_performed;
        input.Player.SelectTrap.performed -= SelectTrap_performed;
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
        mousePosition = callbackContext.ReadValue<Vector2>();
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
            mousePerformed = false;
            Ray mouseRay = Camera.main.ScreenPointToRay(mousePosition, Camera.MonoOrStereoscopicEye.Mono);
            RaycastHit[] hits = new RaycastHit[1];
            Vector3 lookPosition = Vector3.zero;
            if (Physics.RaycastNonAlloc(mouseRay, hits, float.MaxValue, groundLayerMask) > 0)
            {
                lookPosition = hits[0].point;
                lookDirection = (lookPosition - transform.position).normalized;
            }

            Debug.DrawLine(Camera.main.transform.position, lookPosition, Color.magenta);
            Debug.DrawRay(transform.position, lookDirection * (lookPosition - transform.position).magnitude, Color.green);
        }
        return lookDirection;
    }
    
    private void Interaction_performed(InputAction.CallbackContext ctx)
    {
        OnInteract?.Invoke(this, EventArgs.Empty);
    }
    
    private void PlaceTrap_performed(InputAction.CallbackContext ctx)
    {
        OnPlaceTrap?.Invoke(this, EventArgs.Empty);
    }
    
    private void SelectTrap_performed(InputAction.CallbackContext ctx)
    {
        OnSelectTrap?.Invoke(this, (int) ctx.ReadValue<float>());
    }
}
