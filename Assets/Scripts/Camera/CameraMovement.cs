using Mirror;
using Round;
using UnityEngine;

public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private Player testPlayer;
    
    private bool followOpponent = false;
    
    private Player FollowedPlayer()
    {
        if (followOpponent)
            return Player.Opponent;
        if (Player.LocalPlayer != null)
            return Player.LocalPlayer;
        return testPlayer;
    }
    
    private float rotationTime = .5f;

    private int _targetRotation;
    private int _startRotation;
    private float _durationTime;
    private float _currentTime = 0f;
    private float _currentRotation;

    public override void OnStartClient()
    {
        _startRotation = 0;
        _targetRotation = _startRotation;
        _currentRotation = _targetRotation;
        
        if (RoundController.HasLoaded())
            OnRoundLoaded();
        else
            RoundController.OnRoundLoaded += OnRoundLoaded;
    }

    private void OnRoundLoaded()
    {
        RoundController.Instance.OnPlayerKilled += OnPlayerKilled;
        RoundController.Instance.OnPlayerRespawned += OnPlayerRespawned;
    }

    private void OnPlayerKilled(Player player)
    {
        if (Player.LocalPlayer == player)
            followOpponent = true;
    }
    
    private void OnPlayerRespawned(Player player)
    {
        if (Player.LocalPlayer == player)
            followOpponent = false;
    }

    private void OnOnCameraRotation(object sender, int direction)
    {
        _targetRotation += direction * 90;
        _durationTime += rotationTime;
        Debug.Log("----- Target rotation: " + _targetRotation);
    }

    private void LateUpdate()
    {
        if (isServer) return;
        
        Player player = FollowedPlayer();
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
    
    private void OnDestroy()
    {
        RoundController.OnRoundLoaded -= OnRoundLoaded;
    }
}