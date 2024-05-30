using Mirror;
using UnityEngine;

public class MinimapCamera : NetworkBehaviour
{
    [SerializeField] private Player testPlayer;

    public LayerMask minimapLayerMask;
    public float clampOffset = 2f;
    public float darkCheckDistance = 5f;
    public Camera Camera { get; private set; }

    private Vector3 _movementDirection = Vector3.forward;
    private Camera _mainCamera;

    private Player Player => Player.LocalPlayer ? Player.LocalPlayer : testPlayer;

    private void Start()
    {
        _mainCamera = Camera.main;
        Camera = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        Vector3 rayBackOrigin = transform.position - _movementDirection * darkCheckDistance;
        Vector3 rayFrontOrigin = transform.position + _movementDirection * darkCheckDistance;

        Ray rayBack = new Ray(rayBackOrigin, -Vector3.up);
        Ray rayFront = new Ray(rayFrontOrigin, -Vector3.up);

        Debug.DrawRay(rayBackOrigin, -Vector3.up * 50, Color.magenta);
        Debug.DrawRay(rayFrontOrigin, -Vector3.up * 50, Color.magenta);

        bool frontSucceeded = Physics.Raycast(rayFront, out RaycastHit frontHit, float.MaxValue, minimapLayerMask);
        if (!frontSucceeded) return;

        bool backSucceeded = Physics.Raycast(rayBack, out RaycastHit backHit, float.MaxValue, minimapLayerMask);
        if (!backSucceeded) return;

        if (frontHit.collider == backHit.collider &&
            backHit.collider.TryGetComponent(out MinimapDarkArea minimapDarkArea))
        {
            minimapDarkArea.Hide();
        }
    }

    void LateUpdate()
    {
        if (!Player?.transform) return;

        Vector3 newPosition = Player.transform.position;
        newPosition.y = transform.position.y;

        Vector3 newMovementDirection = newPosition - transform.position;
        if (newMovementDirection != Vector3.zero)
        {
            _movementDirection = newMovementDirection.normalized;
        }

        transform.position = newPosition;

        transform.rotation = Quaternion.Euler(90, _mainCamera.transform.rotation.eulerAngles.y, 0);
    }
    
    [ClientRpc]
    public void ShowAllMinimapDarkAreas()
    {
        var minimapDarkAreas = FindObjectsOfType<MinimapDarkArea>(true);
        foreach (var minimapDarkArea in minimapDarkAreas)
        {
            minimapDarkArea.gameObject.SetActive(true);
            minimapDarkArea.Show();
        }
    }
}