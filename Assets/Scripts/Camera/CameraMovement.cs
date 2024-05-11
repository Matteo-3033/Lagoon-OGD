using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Player testPlayer;
    
    private Player player => Player.LocalPlayer == null ? testPlayer : Player.LocalPlayer.gameObject.activeSelf ? Player.LocalPlayer : Player.Opponent;
    
    private float rotationTime = .5f;

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
    }

    private void OnOnCameraRotation(object sender, int direction)
    {
        _targetRotation += direction * 90;
        _durationTime += rotationTime;
        Debug.Log("----- Target rotation: " + _targetRotation);
    }

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.transform.position;
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