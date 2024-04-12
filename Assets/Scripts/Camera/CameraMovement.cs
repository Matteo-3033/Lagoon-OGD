using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public Transform player;

    private Vector3 offset;

    private float _targetRotation;
    private float _startRotation;
    private float _durationTime = 0f;
    private float _currentTime = 0f;
    private float _currentRotation;

    public float rotationTime;

    void Start()
    {
        offset = transform.position - player.position;
        _startRotation = transform.rotation.eulerAngles.y;
        _targetRotation = _startRotation;
        _currentRotation = _targetRotation;
    }

    void LateUpdate()
    {
        Vector3 newPosition = player.position + offset;
        newPosition.y = transform.position.y;

        transform.position = newPosition;

        if (Keyboard.current.eKey.wasPressedThisFrame && _targetRotation < 360)
        {
            _targetRotation += 90;
            _durationTime += rotationTime;
        }

        float t = _currentTime / _durationTime;
        if (1 - t > .001f)
        {
            _currentTime += Time.deltaTime;

            _currentRotation = Mathf.Lerp(_startRotation, _targetRotation, t);
            transform.rotation = Quaternion.Euler(0,
                _currentRotation,
                0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, _targetRotation, 0);
            _startRotation = transform.rotation.eulerAngles.y;
            _targetRotation %= 360;
            _currentRotation = 0;
            _durationTime = 0;
            _currentTime = 0;
        }
    }

    private static float Slope(float x)
    {
        return -2f * Mathf.Pow(x, 3) + 3f * Mathf.Pow(x, 2);
    }
}