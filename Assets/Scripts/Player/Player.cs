using System;
using Interaction;
using MasterServerToolkit.MasterServer;
using Mirror;
using Network.Master;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    [FormerlySerializedAs("MangiagalliBodyGameObject")] [SerializeField] private GameObject MangiagalliBodyPrefab;
    [FormerlySerializedAs("GolgiBodyGameObject")] [SerializeField] private GameObject GolgiBodyPrefab;
    [SerializeField] private Material transparentMaterial;
    private Material defaultMaterial;
    [field: SyncVar(hook = nameof(OnDeadUpdate))] public bool IsDead { get; private set; }

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;
    public event EventHandler<Vector3> OnPositionChanged;

    public static Player LocalPlayer { get; private set; }
    public static Player Opponent { get; private set; }

    public Inventory Inventory => GetComponent<Inventory>();
    public PlayerPositionController PositionController => GetComponent<PlayerPositionController>();
    public PlayerRotationController RotationController => GetComponent<PlayerRotationController>();
    public InputHandler InputHandler => GetComponent<InputHandler>();
    public Interactor Interactor => GetComponentInChildren<Interactor>();
    public TrapSelector TrapSelector => GetComponentInChildren<TrapSelector>();
    public StabManager StabManager => GetComponentInChildren<StabManager>();
    public FieldOfView FieldOfView => GetComponentInChildren<FieldOfView>();
    public MinimapIcon MinimapIcon => GetComponentInChildren<MinimapIcon>();
    public RippleController RippleController => GetComponentInChildren<RippleController>();


    private Vector3 spawnPoint;


    [field: SyncVar] public string Username { get; private set; }

    [field: SyncVar] public bool IsMangiagalli { get; private set; }

    [field: SyncVar] public int Score { get; private set; }

    [field: SyncVar] public int Deaths { get; private set; }

    [field: SyncVar] public int Kills { get; private set; }

    #region SERVER

    [Server]
    public void Init(RoomPlayer profile, bool isMangiagalli)
    {
        Username = profile.Username;
        IsMangiagalli = isMangiagalli;
        Score = profile.Score().Value;
        Deaths = profile.Deaths().Value;
        Kills = profile.Kills().Value;
    }

    [Command]
    private void CmdPositionChanged(Vector3 position)
    {
        OnPositionChanged?.Invoke(this, position);
    }
    
    [Server]
    public void Kill()
    {
        IsDead = true;
    }
    
    [Server]
    public void Respawn()
    {
        IsDead = false;
    }

    #endregion

    #region CLIENT

    public override void OnStartClient()
    {
        base.OnStartClient();

        var identity = gameObject.GetComponent<NetworkIdentity>();

        if (!identity.isLocalPlayer)
            OnStartOpponent();

        spawnPoint = transform.position;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        LocalPlayer = this;

        SetUpModel();
        MakeVisible();
        OnPlayerSpawned?.Invoke(LocalPlayer);
    }

    [Client]
    private void OnStartOpponent()
    {
        Opponent = this;

        SetUpModel();
        MakeInvisible();
        OnPlayerSpawned?.Invoke(Opponent);
    }

    private void SetUpModel()
    {
        GameObject model = IsMangiagalli ? MangiagalliBodyPrefab : GolgiBodyPrefab;

        if (model)
        {
            GetComponentInChildren<MeshRenderer>().gameObject.SetActive(false);
            model = Instantiate(model, transform);
        }

        defaultMaterial = model.GetComponentInChildren<MeshRenderer>().material;
    }

    [TargetRpc]
    public void TargetEnableMovement(bool enable)
    {
        EnableMovement(enable);
    }

    [TargetRpc]
    public void TargetGoTo(Vector3 position)
    {
        transform.position = position;
        CmdPositionChanged(position);
    }

    [Client]
    public void EnableMovement(bool enable)
    {
        PositionController.enabled = enable;
        Interactor.enabled = enable;
    }

    // On client only
    [Client]
    public void MakeInvisible(bool hideIcon = true)
    {
        Layers.SetLayerRecursively(gameObject, Layers.BehindFieldOfView);
        Layers.SetLayerRecursively(RippleController.gameObject, Layers.Minimap);
        if (hideIcon)
            MinimapIcon.Hide();
    }

    // On client only
    [Client]
    public void MakeVisible(bool showIcon = true)
    {
        Layers.SetLayerRecursively(gameObject, Layers.FieldOfView);
        Layers.SetLayerRecursively(RippleController.gameObject, Layers.Minimap);
        if (showIcon)
            MinimapIcon.Show();
    }

    [Command(requiresAuthority = false)]
    public void CmdSetTransparent(bool transparent)
    {
        RpcSetTransparent(transparent);
    }

    [ClientRpc]
    private void RpcSetTransparent(bool transparent)
    {
        GetComponentInChildren<MeshRenderer>().material = transparent ? transparentMaterial : defaultMaterial;
    }

    [Client]
    public override void OnStopClient()
    {
        OnPlayerDespawned?.Invoke(this);
        base.OnStopClient();
    }

    [Client]
    public void SetSilent(bool silent)
    {
        GetComponentInChildren<Footsteps>().SetSilent(silent);
    }

    [Client]
    public void ReturnToSpawn()
    {
        transform.position = spawnPoint;
        CmdPositionChanged(spawnPoint);
    }
    
    [Client]
    public void InvertControls(bool invert)
    {
        InputHandler.Inverted = invert;
    }
    
    [Client]
    private void OnDeadUpdate(bool oldValue, bool newValue)
    {
        if (newValue)
            OnKilled();
        else
            OnRespawned();
    }
    
    [Client]
    private void OnKilled()
    {
        InputHandler.enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Collider>().enabled = false;
        
        for (var i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }
    
    [Client]
    private void OnRespawned()
    {
        for (var i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(true);

        InputHandler.enabled = true;
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().useGravity = true;
        
        if (isLocalPlayer)
            MakeVisible();
        else MakeInvisible();
    }
    
    #endregion

    private void OnDestroy()
    {
        if (isLocalPlayer)
            LocalPlayer = null;
        else
            Opponent = null;
    }

    public void SetCanStealTraps(bool canSteal)
    {
        StabManager.CmdSetCanStealTraps(canSteal);
    }
}