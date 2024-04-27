using System;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    private float rotationTime = .5f;

    private int _targetRotation;
    private int _startRotation;
    private float _durationTime = 0f;
    private float _currentTime = 0f;
    private float _currentRotation;

    private IInputHanlder _inputHandler;

    private void Awake()
    {
    }

    void Start()
    {
        _startRotation = 0;
        _targetRotation = _startRotation;
        _currentRotation = _targetRotation;

        _inputHandler = Player.LocalPlayer?.GetComponent<IInputHanlder>();
    }

    private void OnOnCameraRotation(object sender, int direction)
    {
        _targetRotation += direction * 90;
        _durationTime += rotationTime;
        Debug.Log("----- Target rotation: " + _targetRotation);
    }

    void LateUpdate()
    {
        if (Player.LocalPlayer)
        {
            Vector3 newPosition = Player.LocalPlayer.transform.position;
            newPosition.y = transform.position.y;

            transform.position = newPosition;
        }

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