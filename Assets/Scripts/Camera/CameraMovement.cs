using System;
using Round;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform testTarget = null;

    private Transform Target => Player.LocalPlayer ? Player.LocalPlayer.transform : testTarget?.transform;

    public float rotationTime = .5f;

    private int _targetRotation;
    private int _startRotation;
    private float _durationTime = 0f;
    private float _currentTime = 0f;
    private float _currentRotation;

    void Start()
    {
        _startRotation = 0;
        _targetRotation = _startRotation;
        _currentRotation = _targetRotation;

        IInputHanlder inputHandler = Target?.GetComponent<IInputHanlder>();
        if (inputHandler != null)
        {
            inputHandler.OnCameraRotation += OnOnCameraRotation;
        }
    }

    private void OnValidate()
    {
        IInputHanlder inputHandler = Target?.GetComponent<IInputHanlder>();
        if (inputHandler == null) return;

        inputHandler.OnCameraRotation -= OnOnCameraRotation;
        inputHandler.OnCameraRotation += OnOnCameraRotation;
    }

    private void OnOnCameraRotation(object sender, int direction)
    {
        _targetRotation += direction * 90;
        _durationTime += rotationTime;
    }

    void LateUpdate()
    {
        if (Target)
        {
            Vector3 newPosition = Target.position;
            newPosition.y = transform.position.y;

            transform.position = newPosition;
        }

        float t = _currentTime / _durationTime;
        if (1 - t > .001f)
        {
            _currentTime += Time.deltaTime;
            t = Slope(_currentTime / _durationTime);

            _currentRotation = Mathf.Lerp(_startRotation, _targetRotation, t);
            transform.rotation = Quaternion.Euler(0,
                _currentRotation,
                0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, _targetRotation, 0);
            _targetRotation %= 360;
            _startRotation = _targetRotation;
            _currentRotation = 0;
            _durationTime = 0;
            _currentTime = 0;
        }
    }


    private static float Slope(float x)
    {
        return (1 + Mathf.Cos(Mathf.PI * (x - 1))) / 2;
    }

    private static float InverseSlope(float y)
    {
        return Mathf.Acos(2 * y - 1) / Mathf.PI;
    }
}