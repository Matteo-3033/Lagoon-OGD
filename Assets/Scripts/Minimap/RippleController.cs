using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleController : MonoBehaviour
{
    [SerializeField] private RippleSignal alarmRippleSignal;
    [SerializeField] private RippleSignal keyRippleSignal;

    [Header("Alarm signal Configuration")] public RippleConfiguration alarmRippleConfig;

    [Header("Key signal Configuration")] public RippleConfiguration keyRippleConfig;
    public int minimumKeyNumber = 0;

    private MinimapIcon _minimapIcon;
    private int _keyFragments;

    private bool _isAlarmState;

    public event EventHandler OnAlarmRippleStarted;
    public event EventHandler OnAlarmRippleEnded;

    private void Awake()
    {
        _minimapIcon = GetComponentInChildren<MinimapIcon>();
    }

    void Start()
    {
        Inventory inventory =
            Player.LocalPlayer ? Player.LocalPlayer.Inventory : transform.root.GetComponent<Inventory>();
        inventory.OnKeyFragmentUpdated += OnKeyFragmentUpdated;

        alarmRippleSignal.ConfigureRipple(alarmRippleConfig);
        _isAlarmState = false;

        UpdateKeyRippleEffect(inventory.KeyFragments);
    }

    public void ShowAlarmRipple()
    {
        _isAlarmState = true;
        keyRippleSignal.StopRipple();
        _minimapIcon.StopIconIntermittent();
        alarmRippleSignal.PlayRipple();
        _minimapIcon.ShowIconIntermittent(alarmRippleSignal.RippleLifetime, alarmRippleSignal.Interval);
        OnAlarmRippleStarted?.Invoke(this, EventArgs.Empty);
    }

    public void StopAlarmRipple()
    {
        _isAlarmState = false;
        alarmRippleSignal.StopRipple();
        _minimapIcon.StopIconIntermittent();

        PlayKeyRipple();
        OnAlarmRippleEnded?.Invoke(this, EventArgs.Empty);
    }

    private void PlayKeyRipple()
    {
        if (_keyFragments <= minimumKeyNumber)
        {
            return;
        }

        keyRippleSignal.PlayRipple();
        _minimapIcon.ShowIconIntermittent(keyRippleSignal.RippleLifetime, keyRippleSignal.Interval);
    }

    private void OnKeyFragmentUpdated(object sender, Inventory.OnKeyFragmentUpdatedArgs e)
    {
        UpdateKeyRippleEffect(e.NewValue);
    }

    private void UpdateKeyRippleEffect(int keys)
    {
        _keyFragments = keys;
        if (_keyFragments <= minimumKeyNumber)
        {
            keyRippleSignal.StopRipple();

            return;
        }

        keyRippleSignal.ConfigureRipple(keyRippleConfig.singleRippleDuration,
            keyRippleConfig.interval / _keyFragments,
            keyRippleConfig.scale,
            keyRippleConfig.rippleColor);

        if (_isAlarmState) return;

        _minimapIcon.StopIconIntermittent();
        PlayKeyRipple();
    }
}