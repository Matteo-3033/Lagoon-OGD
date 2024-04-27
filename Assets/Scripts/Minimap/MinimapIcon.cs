using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
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

    private List<SpriteRenderer> _spriteRenderers;
    private bool _isShown;

    public event EventHandler<bool> OnIconShown;
    public event EventHandler<bool> OnIconClamped;
    public event EventHandler OnRippleStarted;
    public event EventHandler OnRippleEnded;

    private void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
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
        if (_spriteRenderers.Count == 0) return;

        _spriteRenderers.ForEach(x => x.enabled = active);
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
        StartCoroutine(EnableClampForSeconds(seconds));
    }

    private IEnumerator EnableClampForSeconds(float seconds)
    {
        bool wasShown = _isShown;
        bool wasClamped = clampToBorder;
        Show();
        ClampToMinimapBorder(true);

        yield return new WaitForSeconds(seconds);

        if (!wasShown) Hide();
        ClampToMinimapBorder(wasClamped);
    }

    public void ShowRipple()
    {
        ShowRipple(defaultRippleConfiguration);
    }

    public void ShowRipple(RippleConfiguration rippleConfiguration)
    {
        ShowRipple(rippleConfiguration.totalDuration,
            rippleConfiguration.repetitions,
            rippleConfiguration.scale,
            rippleConfiguration.rippleColor);
    }

    public void ShowRipple(float totalDuration, float singleRippleDuration, float scale, Color color)
    {
        ShowRipple(totalDuration,
            (int)(totalDuration / singleRippleDuration),
            scale,
            color);
    }

#if !UNITY_EDITOR
    [ClientRpc]
#endif
    public void ShowRipple(float totalDuration, int repetitions, float scale, Color color)
    {
        GameObject rippleObject = Instantiate(ripplePrefab, transform);
        rippleObject.transform.localScale = Vector3.one * scale;
        ParticleSystem rippleEffect = rippleObject.GetComponent<ParticleSystem>();

        ParticleSystem.MainModule rippleEffectMain = rippleEffect.main;
        rippleEffectMain.duration = totalDuration;
        float rippleLifetime = totalDuration / repetitions;
        rippleEffectMain.startLifetime = rippleLifetime;
        rippleEffectMain.startColor = color;

        rippleEffect.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 1, repetitions, rippleLifetime) });

        ClampForSeconds(totalDuration);
        rippleEffect.Play();

        StartCoroutine(RippleEventCoroutine(totalDuration));
    }

    private IEnumerator RippleEventCoroutine(float totalDuration)
    {
        OnRippleStarted?.Invoke(this, EventArgs.Empty);
        yield return new WaitForSeconds(totalDuration);
        OnRippleEnded?.Invoke(this, EventArgs.Empty);
    }
}