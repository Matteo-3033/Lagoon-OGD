using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.SimpleWeb;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class MinimapIcon : NetworkBehaviour
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private bool startHidden;
    [Header("Clamp to border")] public bool clampToBorder = true;
    public float offset;
    [Header("Ripple effect")] public GameObject ripplePrefab;
    public RippleConfiguration defaultRippleConfiguration;

    private List<SimpleIcon> _simpleIcons;
    private bool _isShown;
    private GameObject _currentRippleObject;
    private Coroutine _currentClampIntermittentCoroutine;

    public event EventHandler<bool> OnIconShown;
    public event EventHandler<bool> OnIconClamped;
    public event EventHandler OnRippleStarted;
    public event EventHandler OnRippleEnded;

    private void Awake()
    {
        _simpleIcons = GetComponentsInChildren<SimpleIcon>().ToList();
        if (minimapCamera == null) minimapCamera = GameObject.FindWithTag("MinimapCamera")?.GetComponent<Camera>();

        if (startHidden)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void LateUpdate()
    {
        if (!_isShown || !minimapCamera) return;

        if (!clampToBorder)
        {
            Vector3 parentPosition = transform.parent.position;
            parentPosition.y = transform.position.y;
            transform.position = parentPosition;

            return;
        }

        transform.position = new Vector3(
            Mathf.Clamp(transform.parent.position.x,
                minimapCamera.transform.position.x - minimapCamera.orthographicSize + offset,
                minimapCamera.transform.position.x + minimapCamera.orthographicSize - offset),
            transform.position.y,
            Mathf.Clamp(transform.parent.position.z,
                minimapCamera.transform.position.z - minimapCamera.orthographicSize + offset,
                minimapCamera.transform.position.z + minimapCamera.orthographicSize - offset)
        );
    }

    public void Show()
    {
        SetIconShown(true);
    }

    public void Hide()
    {
        SetIconShown(false);
    }

    private void SetIconShown(bool active)
    {
        if (_simpleIcons.Count == 0) return;

        _simpleIcons.ForEach(x => x.enabled = active);
        _isShown = active;
        OnIconShown?.Invoke(this, active);
    }

    public void ClampToMinimapBorder(bool active)
    {
        clampToBorder = active;
        OnIconClamped?.Invoke(this, active);
    }

    public void ClampForSeconds(float seconds)
    {
        StartCoroutine(EnableShowForSeconds(seconds));
    }

    public Coroutine ShowIconIntermittent(float interval, float clampDuration)
    {
        return StartCoroutine(EnableIntermittentIcon(interval, clampDuration));
    }

    private IEnumerator EnableShowForSeconds(float seconds)
    {
        bool wasShown = _isShown;
        bool wasClamped = clampToBorder;
        Show();
        ClampToMinimapBorder(true);

        yield return new WaitForSeconds(seconds);

        if (!wasShown) Hide();
        ClampToMinimapBorder(wasClamped);
    }

    private IEnumerator EnableIntermittentIcon(float interval, float duration)
    {
        bool wasShown = _isShown;
        bool wasClamped = clampToBorder;

        ParticleSystem ps = _currentRippleObject?.GetComponent<ParticleSystem>();

        ClampToMinimapBorder(true);
        Show();

        Debug.Log("FadeOutIcon: " + ps);
        while (ps && ps.isPlaying)
        {
            _simpleIcons.ForEach(x => x.FadeOutIcon(duration));
            yield return new WaitForSeconds(interval);
        }

        SetIconShown(wasShown);

        _simpleIcons.ForEach(x => x.MakeOpaque());

        ClampToMinimapBorder(wasClamped);
    }


    public void ShowRipple()
    {
        ShowRipple(defaultRippleConfiguration);
    }

    public void ShowRipple(RippleConfiguration rippleConfiguration)
    {
        ShowRipple(rippleConfiguration.singleRippleDuration,
            rippleConfiguration.singleRippleDuration,
            rippleConfiguration.scale,
            rippleConfiguration.rippleColor);
    }

#if !UNITY_EDITOR
    [ClientRpc]
#endif
    public void ShowRipple(float rippleLifetime, float interval, float scale, Color color)
    {
        if (!_currentRippleObject)
        {
            _currentRippleObject = Instantiate(ripplePrefab, transform);
        }

        _currentRippleObject.transform.localScale = Vector3.one * scale;
        ParticleSystem rippleEffect = _currentRippleObject.GetComponent<ParticleSystem>();
        rippleEffect.Stop();

        ParticleSystem.MainModule rippleEffectMain = rippleEffect.main;
        rippleEffectMain.duration = interval;
        rippleEffectMain.startLifetime = rippleLifetime;
        rippleEffectMain.startColor = color;

        rippleEffect.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 1, 0, interval) });

        rippleEffect.Play();
        _currentClampIntermittentCoroutine = ShowIconIntermittent(interval, rippleLifetime);

        OnRippleStarted?.Invoke(this, EventArgs.Empty);
    }

    public void StopRipple()
    {
        _currentRippleObject?.GetComponent<ParticleSystem>()?.Stop();
        if (_currentClampIntermittentCoroutine != null)
        {
            StopCoroutine(_currentClampIntermittentCoroutine);
            _currentClampIntermittentCoroutine = null;
        }

        OnRippleEnded?.Invoke(this, EventArgs.Empty);
    }
}