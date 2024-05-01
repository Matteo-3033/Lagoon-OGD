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

    private SimpleIcon[] _simpleIcons;
    private bool _isShown;
    private GameObject _currentRippleObject;
    private Coroutine _currentClampIntermittentCoroutine;

    public event EventHandler<bool> OnIconShown;
    public event EventHandler<bool> OnIconClamped;

    private void Awake()
    {
        _simpleIcons = GetComponentsInChildren<SimpleIcon>();
        if (minimapCamera == null) minimapCamera = GameObject.FindWithTag("MinimapCamera")?.GetComponent<Camera>();
    }

    private void Start()
    {
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
        if (_simpleIcons.Length == 0) return;

        foreach (SimpleIcon icon in _simpleIcons)
        {
            icon.SetVisible(active);
        }

        _isShown = active;
        OnIconShown?.Invoke(this, active);
    }

    public void ClampToMinimapBorder(bool active)
    {
        clampToBorder = active;
        OnIconClamped?.Invoke(this, active);
    }

    public void ShowIconIntermittent(float fadeDuration, float interval)
    {
        if (fadeDuration <= 0 || interval <= 0)
        {
            Hide();
        }

        _currentClampIntermittentCoroutine = StartCoroutine(EnableIntermittentIcon(fadeDuration, interval));
    }

    public void StopIconIntermittent()
    {
        if (_currentClampIntermittentCoroutine == null) return;

        StopCoroutine(_currentClampIntermittentCoroutine);
        _currentClampIntermittentCoroutine = null;
    }

    private IEnumerator EnableIntermittentIcon(float fadeDuration, float interval)
    {
        ClampToMinimapBorder(true);
        Show();

        while (true)
        {
            foreach (SimpleIcon icon in _simpleIcons)
            {
                icon.FadeOutIcon(fadeDuration);
            }

            yield return new WaitForSeconds(interval);
        }
    }
}