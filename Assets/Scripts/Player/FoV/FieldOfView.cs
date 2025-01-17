using System;
using Mirror;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class FieldOfView : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnFoVDegreeChanged)), SerializeField]
    private float fieldOfViewDegree = 120F;

    [SyncVar(hook = nameof(OnViewDistanceChanged)), SerializeField]
    private float viewDistance = 10F;

    [SyncVar, SerializeField] private int rayCount = 60;
    [SerializeField, Range(0F, 1F)] private float innerFieldDegreePercent = 0.8F;
    [SerializeField, Range(0F, 1F)] private float innerFieldDistancePercent = 0.9F; 
    
    [SerializeField] private LayerMask obstaclesLayermask;
    [SerializeField] private LayerMask consideredLayermask;

    public bool CanSeePlayer
    {
        get;
        private set;
    } // L'avversario nel caso il proprietario del field of view sia il player, altrimenti uno dei due giocatori
    public bool IsPlayerIn { get; private set; }

    public class FieldOfViewArgs
    {
        public float FieldOfViewDegree;
        public float ViewDistance;
    }

    public event EventHandler<FieldOfViewArgs> OnFieldOfViewChanged;

    private float AngleIncrease => fieldOfViewDegree / rayCount;
    private Vector3 Origin => Vector3.zero;
    private float InMaxDistance => innerFieldDistancePercent * viewDistance;
    private int MinInRays => (int)((1 - innerFieldDegreePercent) / 2 * rayCount);

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
        if ((isClient && player && Player.Opponent?.transform == transform.root) || !player)
            return;

        var vertices = new Vector3[rayCount + 2]; // +2 for the origin and the last vertex
        var uv = new Vector2[vertices.Length];
        var triangles = new int[rayCount * 3];

        vertices[0] = Origin;

        var startAngle = Quaternion.AngleAxis(-fieldOfViewDegree / 2, Vector3.up);
        var direction = startAngle * transform.forward;

        var canSeePlayer = false;
        var isPlayerIn = false;

        RaycastHit[] raycastHits = new RaycastHit[5];
        for (var i = 0; i <= rayCount; i++)
        {
            Vector3 newVertex = Origin + viewDistance * transform.worldToLocalMatrix.MultiplyVector(direction);
            int results = Physics.RaycastNonAlloc(transform.position, direction, raycastHits, viewDistance,
                consideredLayermask);

            float minDist = float.MaxValue;
            for (var j = 0; j < results; j++)
            {
                var raycastHit = raycastHits[j];
                GameObject colliderGameObject = raycastHit.collider.gameObject;
                if (raycastHit.distance < minDist && obstaclesLayermask ==
                    (obstaclesLayermask | (1 << colliderGameObject.layer)))
                {
                    minDist = raycastHit.distance;
                    newVertex = transform.worldToLocalMatrix.MultiplyPoint(raycastHit.point);
                }

                canSeePlayer = canSeePlayer || colliderGameObject.TryGetComponent(out Player _);
                if (i > MinInRays && i < rayCount - MinInRays && raycastHit.distance < InMaxDistance)
                {
                    isPlayerIn = isPlayerIn || colliderGameObject.TryGetComponent(out Player _);
                }

                if (i == rayCount / 2)
                {
                    ActivateMinimapIcon(colliderGameObject);
                }
            }

            vertices[i + 1] = newVertex;

            if (i > 0)
            {
                triangles[(i - 1) * 3] = 0;
                triangles[(i - 1) * 3 + 1] = i;
                triangles[(i - 1) * 3 + 2] = i + 1;
            }

            direction = Quaternion.AngleAxis(AngleIncrease, Vector3.up) * direction;
        }

        CanSeePlayer = canSeePlayer;
        IsPlayerIn = isPlayerIn;
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    private void ActivateMinimapIcon(GameObject obj)
    {
        if (!obj || obj == Player.Opponent?.gameObject || obj == Player.LocalPlayer?.gameObject) return;

        MinimapIcon icon = obj.GetComponentInChildren<MinimapIcon>();
        icon?.Show();
    }

    public float GetViewDistance()
    {
        return viewDistance;
    }

    public void SetViewDistance(float dist)
    {
        Debug.Log("Setting distance");
        viewDistance = dist;
        OnFoVUpdated();
    }

    public float GetAngle()
    {
        return fieldOfViewDegree;
    }

    public void SetAngle(float angle)
    {
        Debug.Log("Setting angle");
        fieldOfViewDegree = angle;
        OnFoVUpdated();
    }

    public float GetViewAngle()
    {
        return fieldOfViewDegree;
    }

    [ClientCallback]
    private void OnFoVDegreeChanged(float oldValue, float newValue)
    {
        Debug.Log("Field of view degree changed");
        OnFoVUpdated();
    }

    [ClientCallback]
    private void OnViewDistanceChanged(float oldValue, float newValue)
    {
        Debug.Log("Field distance changed");
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
[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    public void OnSceneGUI()
    {
        var fov = target as FieldOfView;
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