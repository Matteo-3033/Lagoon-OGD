using System;
using Mirror;
using UnityEngine;
using Utils;

public class MinimapIcon : NetworkBehaviour
{
    [SerializeField] private MinimapCamera minimapCamera;
    [SerializeField] private bool startHidden;
    [SerializeField] private bool hideAfterBorder;
    [Header("Clamp to border")] public bool clampToBorder = true;
    public bool circleClamp = false;

    private SimpleIcon[] _simpleIcons;
    private bool _isShown;
    private GameObject _currentRippleObject;
    private Coroutine _currentClampIntermittentCoroutine;

    private Vector3 MinimapCameraPosition => minimapCamera.transform.position;
    private float MinimapCameraOrthographicSize => minimapCamera.Camera.orthographicSize;
    private float ClampOffset => minimapCamera.clampOffset;


    public event EventHandler<bool> OnIconShown;
    public event EventHandler<bool> OnIconClamped;

    private void Awake()
    {
        _simpleIcons = GetComponentsInChildren<SimpleIcon>();
        if (minimapCamera == null)
            minimapCamera = GameObject.FindWithTag("MinimapCamera")?.GetComponent<MinimapCamera>();
    }

    private void Start()
    {
        if (startHidden)
            Hide();
        else
            Show();
    }

    private void LateUpdate()
    {
        if (!_isShown || !minimapCamera) return;

        if (hideAfterBorder)
        {
            Vector3 iconPosition = transform.position;
            iconPosition.y = 0;
            Vector3 cameraPosition = MinimapCameraPosition;
            cameraPosition.y = 0;
            float orthographicSize = MinimapCameraOrthographicSize;
            if((iconPosition - cameraPosition).magnitude > orthographicSize + 3) Hide();
        }

        if (!clampToBorder)
        {
            Vector3 parentPosition = transform.parent.position;
            parentPosition.y = transform.position.y;
            transform.position = parentPosition;

            return;
        }

        if (circleClamp)
        {
            CircleClamp();
        }
        else
        {
            SquareClamp();
        }
    }

    private void SquareClamp()
    {
        transform.position = new Vector3(
            Mathf.Clamp(transform.parent.position.x,
                MinimapCameraPosition.x - MinimapCameraOrthographicSize + ClampOffset,
                MinimapCameraPosition.x + MinimapCameraOrthographicSize - ClampOffset),
            transform.position.y,
            Mathf.Clamp(transform.parent.position.z,
                MinimapCameraPosition.z - MinimapCameraOrthographicSize + ClampOffset,
                MinimapCameraPosition.z + MinimapCameraOrthographicSize - ClampOffset)
        );
    }

    private void CircleClamp()
    {
        Vector3 centerPosition = MinimapCameraPosition;
        centerPosition.y = transform.parent.position.y;

        Vector3 toParent = transform.parent.position - centerPosition;

        if (toParent.magnitude < MinimapCameraOrthographicSize - ClampOffset)
        {
            transform.position = new Vector3(transform.parent.position.x,
                transform.position.y,
                transform.parent.position.z
            );
            return;
        }

        Vector3 newPosition = minimapCamera.transform.position +
                              toParent.normalized * (MinimapCameraOrthographicSize - ClampOffset);
        transform.position = new Vector3(newPosition.x,
            transform.position.y,
            newPosition.z);
        Debug.DrawRay(minimapCamera.transform.position, toParent.normalized * MinimapCameraOrthographicSize,
            Color.cyan);
    }

    public void Show()
    {
        SetIconShown(true);
        Layers.SetLayerRecursively(gameObject, Layers.Default);
    }

    public void Hide()
    {
        if (Player.LocalPlayer?.transform == transform.root) return;

        SetIconShown(false);
    }
    
    [ClientRpc]
    public void RpcShow()
    {
        Show();
    }

    [ClientRpc]
    public void RpcHide()
    {
        Hide();
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

    public void PlayIconFade(float fadeDuration)
    {
        if (Player.LocalPlayer?.transform == transform.root) return;

        if (fadeDuration <= 0)
        {
            Hide();
        }

        foreach (SimpleIcon icon in _simpleIcons)
        {
            icon.FadeOutIcon(fadeDuration);
        }
    }
}