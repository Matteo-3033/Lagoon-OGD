using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private bool showAtStart;
    [Header("Clamp to border")] public bool clampToBorder = true;
    public float offset;
    [Header("Ripple effect")] public GameObject ripplePrefab;

    private SpriteRenderer _spriteRenderer;
    private bool _isShown;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (showAtStart)
        {
            Show();
        }
        else
        {
            Hide();
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
        if (!_spriteRenderer) return;

        _spriteRenderer.enabled = true;
        _isShown = true;
    }

    public void Hide()
    {
        if (!_spriteRenderer) return;

        _spriteRenderer.enabled = false;
        _isShown = false;
    }

    public void ClampToMinimapBorder(bool active)
    {
        clampToBorder = active;
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

    public void ShowRipple(RippleConfiguration rippleConfiguration)
    {
        ShowRipple(rippleConfiguration.totalDuration,
            rippleConfiguration.repetitions,
            rippleConfiguration.scale,
            rippleConfiguration.rippleColor);
    }

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
    }
}