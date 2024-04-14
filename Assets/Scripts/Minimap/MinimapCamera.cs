using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public Transform player;
    public LayerMask minimapLayerMask;
    public float darkCheckDistance = 5f;

    private Vector3 _movementDirection = Vector3.forward;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void FixedUpdate()
    {
        Vector3 rayBackOrigin = transform.position - _movementDirection * darkCheckDistance;
        Vector3 rayFrontOrigin = transform.position + _movementDirection * darkCheckDistance;
        Ray rayBack = new Ray(rayBackOrigin, -Vector3.up);
        Ray rayFront = new Ray(rayFrontOrigin, -Vector3.up);
        Debug.DrawRay(rayBackOrigin, -Vector3.up * 50, Color.magenta);
        Debug.DrawRay(rayFrontOrigin, -Vector3.up * 50, Color.magenta);

        bool backSucceeded = Physics.Raycast(rayBack, out RaycastHit backHit, float.MaxValue, minimapLayerMask);
        bool frontSucceeded = Physics.Raycast(rayFront, out RaycastHit frontHit, float.MaxValue, minimapLayerMask);
        if (backSucceeded && frontSucceeded)
        {
            if (frontHit.collider == backHit.collider &&
                backHit.collider.TryGetComponent(out MinimapDarkArea minimapDarkArea))
            {
                minimapDarkArea.Hide();
            }
        }
    }

    void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;

        Vector3 newMovementDirection = newPosition - transform.position;
        if (newMovementDirection != Vector3.zero)
        {
            _movementDirection = newMovementDirection.normalized;
        }

        transform.position = newPosition;

        transform.rotation = Quaternion.Euler(90, _camera.transform.rotation.eulerAngles.y, 0);
    }
}