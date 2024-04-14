using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    [SerializeField] private Camera minimapCamera;
    [Space]
    public bool clampToBorder = true;
    public float offset = 0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (!minimapCamera) return;

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
    
    
}