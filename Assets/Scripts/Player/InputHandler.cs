using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public interface IInputHanlder
{
    event EventHandler<int> OnCameraRotation;
    Vector3 GetMovementDirection();
    Vector3 GetLookDirection();
}

public class InputHandler : MonoBehaviour, IInputHanlder
{
    public LayerMask groundLayerMask;
    [Range(0,1)]
    private float cameraMovementIgnoreTime = .8f;

    public event EventHandler<int> OnCameraRotation;

    private CustomInput input = null;

    private Vector3 inputMovementDirection;
    private Vector3 lookDirection;
    private Vector3 mousePosition;
    private bool mousePerformed;
    private Camera _camera;
    private float _timeSinceLastCameraRotation;

    public delegate void Move(Vector3 inputDirection);

    private void Awake()
    {
        input = new CustomInput();
        _camera = Camera.main;
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
        if (!mousePerformed)
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

    private void CameraRotationMousePerformed(InputAction.CallbackContext callbackContext)
    {
        if (!CanPerformCameraRotation()) return;

        OnCameraRotation?.Invoke(this, (int)callbackContext.ReadValue<float>());
    }

    private void CameraRotationGamepadPerformed(InputAction.CallbackContext callbackContext)
    {
        if (!CanPerformCameraRotation()) return;

        OnCameraRotation?.Invoke(this, callbackContext.ReadValueAsButton() ? 1 : -1);
    }

    private bool CanPerformCameraRotation()
    {
        bool canPerform = Time.time - _timeSinceLastCameraRotation >= cameraMovementIgnoreTime;
        if (canPerform) _timeSinceLastCameraRotation = Time.time;

        return canPerform;
    }

    #region Enable/disable

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += Movement_performed;
        input.Player.Movement.canceled += Movement_canceled;

        input.Player.MousePosition.performed += MousePosition_performed;
        input.Player.View.performed += View_performed;

        input.Player.CameraRotationMouse.performed += CameraRotationMousePerformed;
        input.Player.CameraRotationGamepad.performed += CameraRotationGamepadPerformed;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= Movement_performed;
        input.Player.Movement.canceled -= Movement_canceled;

        input.Player.MousePosition.performed -= MousePosition_performed;
        input.Player.View.performed -= View_performed;

        input.Player.CameraRotationMouse.performed -= CameraRotationMousePerformed;
        input.Player.CameraRotationGamepad.performed -= CameraRotationGamepadPerformed;
    }

    #endregion
}