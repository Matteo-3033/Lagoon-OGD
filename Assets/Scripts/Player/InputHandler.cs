using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInputHanlder
{
    Vector3 GetMovementDirection();
    Vector3 GetLookDirection();

    public event EventHandler<bool> OnInteract;
    public event EventHandler<EventArgs> OnPlaceTrap;
    public event EventHandler<int> OnSelectTrap;
    public event EventHandler<int> OnCameraRotation;
    public bool Inverted { get; set; }
}

public class InputHandler : MonoBehaviour, IInputHanlder
{
    public LayerMask groundLayerMask;

    [Range(0, 1)] public float cameraMovementIgnoreTime = .8f;

    private CustomInput input = null;

    private Vector3 inputMovementDirection;
    private Vector3 lookDirection;
    private Vector3 mousePosition;
    private bool mousePerformed;
    private Camera _camera;
    private float _timeSinceLastCameraRotation;

    public delegate void Move(Vector3 inputDirection);
    
    public event EventHandler<bool> OnInteract;
    public event EventHandler<EventArgs> OnPlaceTrap;
    public event EventHandler<int> OnSelectTrap;
    public event EventHandler<int> OnCameraRotation;
    public bool Inverted { get; set; }

    private void Awake()
    {
        input = new CustomInput();
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += Movement_performed;
        input.Player.Movement.canceled += Movement_canceled;

        input.Player.MousePosition.performed += MousePosition_performed;
        input.Player.View.performed += View_performed;

        input.Player.Interaction.performed += Interaction;
        input.Player.Interaction.canceled += Interaction;
        
        input.Player.PlaceTrap.performed += PlaceTrap_performed;
        input.Player.SelectTrap.performed += SelectTrap_performed;

        input.Player.CameraRotation.performed += CameraRotation_performed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= Movement_performed;
        input.Player.Movement.canceled -= Movement_canceled;

        input.Player.MousePosition.performed -= MousePosition_performed;
        input.Player.View.performed -= View_performed;

        input.Player.Interaction.performed -= Interaction;
        input.Player.Interaction.canceled -= Interaction;
        
        input.Player.PlaceTrap.performed -= PlaceTrap_performed;
        input.Player.SelectTrap.performed -= SelectTrap_performed;

        input.Player.CameraRotation.performed += CameraRotation_performed;
    }

    private void Movement_performed(InputAction.CallbackContext callbackContext)
    {
        Vector2 temp = callbackContext.ReadValue<Vector2>();
        inputMovementDirection = !Inverted ? new Vector3(temp.x, 0F, temp.y) : new Vector3(-temp.x, 0F, -temp.y);
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
        return inputMovementDirection.x * _camera.transform.parent.right + inputMovementDirection.z * _camera.transform.parent.forward;
    }

    public Vector3 GetLookDirection()
    {
        if (!mousePerformed || !_camera)
        {
            return lookDirection;
        }

        mousePerformed = false;
        Ray mouseRay = _camera.ScreenPointToRay(mousePosition, Camera.MonoOrStereoscopicEye.Mono);
        RaycastHit[] hits = new RaycastHit[1];
        Vector3 lookPosition = Vector3.zero;
        if (Physics.RaycastNonAlloc(mouseRay, hits, float.MaxValue, groundLayerMask) > 0)
        {
            lookPosition = hits[0].point;
            lookDirection = (lookPosition - transform.position).normalized;
        }

        Debug.DrawLine(_camera.transform.position, lookPosition, Color.magenta);
        Debug.DrawRay(transform.position, lookDirection * (lookPosition - transform.position).magnitude, Color.green);
        return lookDirection;
    }
    
    private void Interaction(InputAction.CallbackContext ctx)
    {
        OnInteract?.Invoke(this, ctx.performed);
    }
    
    private void PlaceTrap_performed(InputAction.CallbackContext ctx)
    {
        OnPlaceTrap?.Invoke(this, EventArgs.Empty);
    }
    
    private void SelectTrap_performed(InputAction.CallbackContext ctx)
    {
        OnSelectTrap?.Invoke(this, (int) ctx.ReadValue<float>());
    }

    private void CameraRotation_performed(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Can rotation");
        if (!CanPerformCameraRotation()) return;

        OnCameraRotation?.Invoke(this, callbackContext.ReadValueAsButton() ? 1 : -1);
    }

    private bool CanPerformCameraRotation()
    {
        bool canPerform = Time.time - _timeSinceLastCameraRotation >= cameraMovementIgnoreTime;
        if (canPerform) _timeSinceLastCameraRotation = Time.time;

        Debug.Log("Can perform rotation: " + canPerform);
        return canPerform;
    }
}