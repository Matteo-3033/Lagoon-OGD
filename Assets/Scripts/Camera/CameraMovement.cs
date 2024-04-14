using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    public float rotationTime;

    private int _targetRotation;
    private int _startRotation;
    private float _durationTime = 0f;
    private float _currentTime = 0f;
    private float _currentRotation;

    private IInputHanlder _inputHandler;

    private void Awake()
    {
        _inputHandler = player.GetComponent<IInputHanlder>();
    }

    void Start()
    {
        _startRotation = 0;
        _targetRotation = _startRotation;
        _currentRotation = _targetRotation;

        _inputHandler.OnCameraRotation += OnOnCameraRotation;
    }

    private void OnOnCameraRotation(object sender, int direction)
    {
        _targetRotation += direction * 90;
        _durationTime += rotationTime;
        Debug.Log("----- Target rotation: " + _targetRotation);
    }

    void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;

        transform.position = newPosition;

        float t = _currentTime / _durationTime;
        if (1 - t > .001f)
        {
            _currentTime += Time.deltaTime;
            //t = Slope(_currentTime / _durationTime);

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