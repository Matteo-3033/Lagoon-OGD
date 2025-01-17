using System;
using Interaction;
using MasterServerToolkit.MasterServer;
using Mirror;
using Network.Master;
using UnityEngine;
using Utils;
using WebSocketSharp;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    [SerializeField] private Material transparentMaterial;
    
    [SerializeField] private GameObject defaultBody;
    [SerializeField] private GameObject mangiagalliBody;
    [SerializeField] private GameObject golgiBody;
    private GameObject body;
    
    public bool IsDead { get; private set; }

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDeSpawned;
    public event EventHandler<Vector3> OnPositionChanged;

    public static Player LocalPlayer { get; private set; }
    public static Player Opponent { get; private set; }

    public Inventory Inventory => GetComponent<Inventory>();
    public PlayerPositionController PositionController => GetComponent<PlayerPositionController>();
    public PlayerRotationController RotationController => GetComponent<PlayerRotationController>();
    public InputHandler InputHandler => GetComponent<InputHandler>();
    public Interactor Interactor => GetComponentInChildren<Interactor>(true);
    public TrapSelector TrapSelector => GetComponentInChildren<TrapSelector>(true);
    public StabManager StabManager => GetComponent<StabManager>();
    public FieldOfView FieldOfView => GetComponentInChildren<FieldOfView>(true);
    public MinimapIcon MinimapIcon => GetComponentInChildren<MinimapIcon>(true);
    public RippleController RippleController => GetComponentInChildren<RippleController>(true);
    public PlayerAnimationManager AnimationManager => GetComponentInChildren<PlayerAnimationManager>(true);


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

    public override void OnStartServer()
    {
        base.OnStartServer();
        SetUpModel();
    }

    [Command(requiresAuthority = false)]
    private void CmdPositionChanged(Vector3 position)
    {
        OnPositionChanged?.Invoke(this, position);
    }

    [Server]
    public void Kill()
    {
        IsDead = true;
        RpcOnKilled();
    }

    [Server]
    public void Respawn()
    {
        IsDead = false;
        RpcOnRespawned();
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
        var model = IsMangiagalli ? mangiagalliBody : golgiBody;
        var otherModel = IsMangiagalli ? golgiBody : mangiagalliBody;

        if (!model)
        {
            body = defaultBody;
            return;
        }

        body = model;
        model.SetActive(true);
        
        Debug.Log("Player " + Username + " is " + (IsMangiagalli ? "Mangiagalli" : "Golgi") + "!");
        
        Destroy(defaultBody);
        Destroy(otherModel);

        var networkAnimator = GetComponent<NetworkAnimator>();
        networkAnimator.animator = model.GetComponent<Animator>();
        networkAnimator.Initialize();
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
        StabManager.enabled = enable;
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
        var mats = GetComponentInChildren<SkinnedMeshRenderer>().materials;
        Material[] newMats;

        if (transparent)
        {
            if (mats.Length > 0 && mats[0] == transparentMaterial)
                return;

            newMats = new Material[mats.Length + 1];
            newMats[0] = transparentMaterial;
            for (var i = 0; i < mats.Length; i++)
                newMats[i + 1] = mats[i];
        }
        else if (mats.Length > 0 && mats[0] == transparentMaterial)
        {
            if (mats.Length > 0)
                newMats = mats.SubArray(1, mats.Length - 1);
            else newMats = mats;
        }
        else return;

        GetComponentInChildren<SkinnedMeshRenderer>().materials = newMats;
    }

    [Client]
    public override void OnStopClient()
    {
        OnPlayerDeSpawned?.Invoke(this);
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
        Debug.Log("Returning to spawn");
        transform.position = spawnPoint;
        CmdPositionChanged(spawnPoint);
    }

    [Client]
    public void InvertControls(bool invert)
    {
        InputHandler.Inverted = invert;
    }

    [ClientRpc]
    private void RpcOnKilled()
    {
        Debug.Log("Player " + Username + " killed");
        IsDead = true;
        
        InputHandler.enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Collider>().enabled = false;
        
        for (var i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject != body)
                transform.GetChild(i).gameObject.SetActive(false);
        }
        
        PlayerAnimationEvents.OnAnimationEnded += OnAnimationEnded;
    }

    [ClientCallback]
    private void OnAnimationEnded(object sender, PlayerAnimationManager.Animation animation)
    {
        if (animation == PlayerAnimationManager.Animation.Death)
            OnKillCompleted();
    }

    [Client]
    private void OnKillCompleted()
    {
        PlayerAnimationEvents.OnAnimationEnded -= OnAnimationEnded;
        Debug.Log("KILL COMPLETED");
        body.SetActive(false);
        ReturnToSpawn();
    }

    [ClientRpc]
    private void RpcOnRespawned()
    {
        Debug.Log("Player " + Username + " respawned");
        IsDead = false;

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