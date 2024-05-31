using System;
using Interaction;
using MasterServerToolkit.MasterServer;
using Mirror;
using Network.Master;
using UnityEngine;
using Utils;

[RequireComponent(typeof(NetworkIdentity)), RequireComponent(typeof(MeshRenderer))]
public class Player : NetworkBehaviour
{
    [SerializeField] private Material transparentMaterial;
    private Material defaultMaterial;

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;
    public event EventHandler<Vector3> OnPositionChanged;

    public static Player LocalPlayer { get; private set; }
    public static Player Opponent { get; private set; }


    private Inventory _inventory;
    public Inventory Inventory => _inventory ? _inventory : _inventory = GetComponent<Inventory>();

    private PlayerPositionController _positionController;

    public PlayerPositionController PositionController => _positionController
        ? _positionController
        : _positionController = GetComponent<PlayerPositionController>();

    private PlayerRotationController _rotationController;

    public PlayerRotationController RotationController => _rotationController
        ? _rotationController
        : _rotationController = GetComponent<PlayerRotationController>();

    private TrapSelector _trapSelector;
    public TrapSelector TrapSelector => _trapSelector ? _trapSelector : _trapSelector = GetComponent<TrapSelector>();

    private Interactor _interactor;
    public Interactor Interactor => _interactor ? _interactor : _interactor = GetComponent<Interactor>();

    private FieldOfView _fieldOfView;

    public FieldOfView FieldOfView =>
        _fieldOfView ? _fieldOfView : _fieldOfView = GetComponentInChildren<FieldOfView>();

    private RippleController _rippleController;

    public RippleController RippleController => _rippleController
        ? _rippleController
        : _rippleController = GetComponentInChildren<RippleController>();

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

    #endregion

    #region CLIENT

    public override void OnStartClient()
    {
        base.OnStartClient();

        var identity = gameObject.GetComponent<NetworkIdentity>();

        if (!identity.isLocalPlayer)
            OnStartOpponent();

        defaultMaterial = GetComponent<MeshRenderer>().material;
        spawnPoint = transform.position;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        LocalPlayer = this;

        MakeVisible();
        OnPlayerSpawned?.Invoke(LocalPlayer);
    }

    [Client]
    private void OnStartOpponent()
    {
        Opponent = this;

        MakeInvisible();
        OnPlayerSpawned?.Invoke(Opponent);
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
    public void MakeInvisible()
    {
        Layers.SetLayerRecursively(gameObject, Layers.BehindFieldOfView);
        gameObject.GetComponentInChildren<MinimapIcon>().Hide();
    }

    // On client only
    [Client]
    public void MakeVisible()
    {
        Layers.SetLayerRecursively(gameObject, Layers.FieldOfView);
        gameObject.GetComponentInChildren<MinimapIcon>().Show();
    }

    [Command(requiresAuthority = false)]
    public void CmdSetTransparent(bool transparent)
    {
        RpcSetTransparent(transparent);
    }

    [ClientRpc]
    private void RpcSetTransparent(bool transparent)
    {
        GetComponent<MeshRenderer>().material = transparent ? transparentMaterial : defaultMaterial;
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

    #endregion
}