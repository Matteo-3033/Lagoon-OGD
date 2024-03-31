using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class FieldOfVIew : MonoBehaviour
{
    [SerializeField] private float fieldOfView = 120F;
    [SerializeField] private int rayCount = 60;
    [SerializeField] private float viewDistance = 10F;
    
    private float AngleIncrease => fieldOfView / rayCount;
    private Vector3 Origin => Vector3.zero;

    private Mesh mesh;
    
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Update()
    {
        var vertices = new Vector3[rayCount + 2];   // +2 for the origin and the last vertex
        var uv = new Vector2[vertices.Length];
        var triangles = new int[rayCount * 3];
        
        vertices[0] = Origin;

        var startAngle = Quaternion.AngleAxis(-fieldOfView / 2, Vector3.up);
        var localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);
        var direction = startAngle * localForward;
        
        for (var i = 0; i <= rayCount; i++)
        {
            Vector3 newVertex;
            if (Physics.Raycast(transform.position, direction, out var hit, viewDistance))
                newVertex = transform.worldToLocalMatrix.MultiplyPoint(hit.point);
            else newVertex = Origin + viewDistance * direction;

            vertices[i + 1] = newVertex;

            if (i > 0)
            {
                triangles[(i - 1) * 3] = 0;
                triangles[(i - 1) * 3 + 1] = i;
                triangles[(i - 1) * 3 + 2] = i + 1;
            }

            direction = Quaternion.AngleAxis(AngleIncrease, Vector3.up) * direction;
        }
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
    
    public void SetFoV(float fov)
    {
        this.fieldOfView = fov;
    }
    
    public void SetViewDistance(float dist)
    {
        this.viewDistance = dist;
    }
}
