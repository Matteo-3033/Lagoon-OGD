using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class FieldOfVIew : MonoBehaviour
{
    [SerializeField] private float fieldOfViewDegree = 120F;
    [SerializeField] private int rayCount = 60;
    [SerializeField] private float viewDistance = 10F;

    public class FieldOfViewArgs
    {
        public float FieldOfViewDegree;
        public float ViewDistance;
    }

    public event EventHandler<FieldOfViewArgs> OnFieldOfViewChanged;
    
    private float AngleIncrease => fieldOfViewDegree / rayCount;
    private Vector3 Origin => Vector3.zero;

    private Mesh mesh;
    
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        OnFoVUpdated();
    }
    
    private void Update()
    {
        var vertices = new Vector3[rayCount + 2];   // +2 for the origin and the last vertex
        var uv = new Vector2[vertices.Length];
        var triangles = new int[rayCount * 3];
        
        vertices[0] = Origin;

        var startAngle = Quaternion.AngleAxis(-fieldOfViewDegree / 2, Vector3.up);
        var direction = startAngle * transform.forward;
        
        for (var i = 0; i <= rayCount; i++)
        {
            Vector3 newVertex;
            if (Physics.Raycast(transform.position, direction, out var hit, viewDistance))
                newVertex = transform.worldToLocalMatrix.MultiplyPoint(hit.point);
            else newVertex = Origin + viewDistance * transform.worldToLocalMatrix.MultiplyVector(direction);

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
    
    public void SetViewDistance(float dist)
    {
        this.viewDistance = dist;
        OnFoVUpdated();
    }

    private void OnFoVUpdated()
    {
        OnFieldOfViewChanged?.Invoke(this, new FieldOfViewArgs {FieldOfViewDegree = fieldOfViewDegree, ViewDistance = viewDistance});
    }
}
