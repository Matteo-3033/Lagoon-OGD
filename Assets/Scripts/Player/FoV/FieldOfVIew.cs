using System;
using Mirror;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class FieldOfVIew : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnFoVDegreeChanged)), SerializeField]
    private float fieldOfViewDegree = 120F;

    [SyncVar(hook = nameof(OnViewDistanceChanged)), SerializeField]
    private float viewDistance = 10F;

    [SyncVar, SerializeField] private int rayCount = 60;

    public class FieldOfViewArgs
    {
        public float FieldOfViewDegree;
        public float ViewDistance;
    }

    public event EventHandler<FieldOfViewArgs> OnFieldOfViewChanged;

    private float AngleIncrease => fieldOfViewDegree / rayCount;
    private Vector3 Origin => Vector3.zero;

    private Mesh mesh;
    private Player player;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        player = GetComponentInParent<Player>();
    }

    public override void OnStartClient()
    {
        base.OnStartServer();

        OnFoVUpdated();
    }

    private void Update()
    {
        if (isClient && player && !player.isLocalPlayer)
            return;

        var vertices = new Vector3[rayCount + 2]; // +2 for the origin and the last vertex
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

    [Server]
    public void SetViewDistance(float dist)
    {
        viewDistance = dist;
    }

    [Server]
    public void SetAngle(float angle)
    {
        fieldOfViewDegree = angle;
    }

    public float GetViewAngle()
    {
        return fieldOfViewDegree;
    }

    public float GetViewDistance()
    {
        return viewDistance;
    }

    [ClientCallback]
    private void OnFoVDegreeChanged(float oldValue, float newValue)
    {
        OnFoVUpdated();
    }

    [ClientCallback]
    private void OnViewDistanceChanged(float oldValue, float newValue)
    {
        OnFoVUpdated();
    }

    [ClientCallback]
    private void OnFoVUpdated()
    {
        OnFieldOfViewChanged?.Invoke(this,
            new FieldOfViewArgs { FieldOfViewDegree = fieldOfViewDegree, ViewDistance = viewDistance });
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FieldOfVIew))]
public class FieldOfViewEditor : Editor
{
    public void OnSceneGUI()
    {
        var fov = target as FieldOfVIew;
        if (!fov) return;

        var transform = fov.transform;
        var pos = transform.position;

        float angle = fov.GetViewAngle();
        float radius = fov.GetViewDistance();

        float startAngle = transform.eulerAngles.y - angle / 2;
        float endAngle = transform.eulerAngles.y + angle / 2;
        Vector3 startDir = new Vector3(Mathf.Sin(startAngle * Mathf.Deg2Rad), 0, Mathf.Cos(startAngle * Mathf.Deg2Rad));
        Vector3 endDir = new Vector3(Mathf.Sin(endAngle * Mathf.Deg2Rad), 0, Mathf.Cos(endAngle * Mathf.Deg2Rad));

        Handles.color = Color.white;
        Handles.DrawWireArc(pos,
            transform.up,
            startDir,
            angle,
            radius);
        Handles.DrawLine(transform.position, transform.position + startDir * radius);
        Handles.DrawLine(transform.position, transform.position + endDir * radius);
    }
}
#endif