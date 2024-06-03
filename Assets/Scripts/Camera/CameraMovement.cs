using Mirror;
using Round;
using UnityEngine;

public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private Player testTarget = null;

    private bool _followOpponent = false;

    private Transform Target()
    {
        if (_followOpponent)
            return Player.Opponent.transform;

        return Player.LocalPlayer ? Player.LocalPlayer.transform : testTarget?.transform;
    }

    private float rotationTime = .5f;

    private int _targetRotation;
    private int _startRotation;
    private float _durationTime = 0f;
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
        KillController.OnPlayerKilled += OnPlayerKilled;
        KillController.OnPlayerRespawned += OnPlayerRespawned;

        var inputHandler = Target()?.GetComponent<IInputHandler>();
        if (inputHandler == null) return;
        inputHandler.OnCameraRotation += OnOnCameraRotation;
    }

    private void OnPlayerKilled(Player player)
    {
        if (!player.isLocalPlayer)
            return;

        _followOpponent = true;
        Player.Opponent.MakeVisible(false);
    }

    private void OnPlayerRespawned(Player player)
    {
        if (!player.isLocalPlayer)
            return;

        _followOpponent = false;
        Player.Opponent.MakeInvisible(false);
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        var inputHandler = Target()?.GetComponent<IInputHandler>();
        if (inputHandler == null) return;

        inputHandler.OnCameraRotation -= OnOnCameraRotation;
        inputHandler.OnCameraRotation += OnOnCameraRotation;
    }

    private void OnOnCameraRotation(object sender, int direction)
    {
        _targetRotation += direction * 90;
        _durationTime += rotationTime;
    }

    private void LateUpdate()
    {
        if (isServer) return;

        var target = Target();
        if (target)
        {
            Vector3 newPosition = Target().position;
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

    private void OnDestroy()
    {
        RoundController.OnRoundLoaded -= OnRoundLoaded;
        KillController.OnPlayerKilled -= OnPlayerKilled;
        KillController.OnPlayerRespawned -= OnPlayerRespawned;
    }
}