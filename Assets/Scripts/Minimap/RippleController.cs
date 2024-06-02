using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class RippleController : MonoBehaviour
{
    [SerializeField] private RippleSignal alarmRippleSignal;
    [SerializeField] private RippleSignal keyRippleSignal;

    [Header("Alarm signal Configuration")] public RippleConfiguration alarmRippleConfig;

    [Header("Key signal Configuration")] public RippleConfiguration keyRippleConfig;
    [FormerlySerializedAs("minimumKeyNumber")] public int minimumBadgeNumber = 0;

    private MinimapIcon _minimapIcon;
    private int _keyFragments;

    private bool _isAlarmState;
    private Coroutine _currentRippleCoroutine;

    public event EventHandler OnAlarmRippleStarted;
    public event EventHandler OnAlarmRippleEnded;

    private void Awake()
    {
        _minimapIcon = GetComponentInChildren<MinimapIcon>();
    }

    void Start()
    {
        var inventory = GetComponentInParent<Player>().Inventory;
        inventory.OnKeyFragmentUpdated += OnKeyFragmentUpdated;

        alarmRippleSignal.ConfigureRipple(alarmRippleConfig);
        _isAlarmState = false;

        UpdateKeyRippleEffect(inventory.KeyFragments);
    }

    public void ShowAlarmRipple()
    {
        _isAlarmState = true;
        StopCurrentRipple();
        PlayAlarmRipple();
        OnAlarmRippleStarted?.Invoke(this, EventArgs.Empty);
    }

    public void StopAlarmRipple()
    {
        _isAlarmState = false;
        StopCurrentRipple();
        PlayKeyRipple();
        OnAlarmRippleEnded?.Invoke(this, EventArgs.Empty);
    }

    private void PlayKeyRipple()
    {
        if (_keyFragments <= minimumBadgeNumber) return;

        _currentRippleCoroutine = StartCoroutine(RippleLoop(keyRippleSignal));
    }

    private void PlayAlarmRipple()
    {
        _currentRippleCoroutine = StartCoroutine(RippleLoop(alarmRippleSignal));
    }

    private void StopCurrentRipple()
    {
        if (_currentRippleCoroutine == null) return;

        StopCoroutine(_currentRippleCoroutine);
        _currentRippleCoroutine = null;
    }

    private void OnKeyFragmentUpdated(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
    {
        UpdateKeyRippleEffect(args.NewValue);
    }

    private void UpdateKeyRippleEffect(int keys)
    {
        _keyFragments = keys;

        if (!_isAlarmState && _keyFragments <= minimumBadgeNumber)
        {
            StopCurrentRipple();
            _minimapIcon.Hide();
            return;
        }

        keyRippleSignal.ConfigureRipple(keyRippleConfig.singleRippleDuration,
            keyRippleConfig.interval / _keyFragments,
            keyRippleConfig.scale,
            keyRippleConfig.rippleColor);

        if (_currentRippleCoroutine == null) PlayKeyRipple();
    }

    private IEnumerator RippleLoop(RippleSignal rippleSignal)
    {
        while (true)
        {
            rippleSignal.PlayRipple();
            _minimapIcon.PlayIconFade(rippleSignal.RippleLifetime / 2);
            yield return new WaitForSeconds(rippleSignal.Interval);
        }
    }
}