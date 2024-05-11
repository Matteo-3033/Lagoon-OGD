using System;
using Interaction;
using MasterServerToolkit.MasterServer;
using Mirror;
using Network.Master;
using Round;
using UnityEngine;
using Utils;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;
    public event Action<Player> OnPlayerRespawned;
    public event Action<Player> OnPlayerKilled;
    public event EventHandler<Vector3> OnPositionChanged;
    
    public static Player LocalPlayer { get; private set;  }
    public static Player Opponent { get; private set;  }

    public Inventory Inventory => GetComponent<Inventory>();
    public PlayerPositionController PositionController => GetComponent<PlayerPositionController>();
	public PlayerRotationController RotationController => GetComponent<PlayerRotationController>();
    public TrapSelector TrapSelector => GetComponent<TrapSelector>();
    public Interactor Interactor => GetComponent<Interactor>();
    public InputHandler InputHandler => GetComponent<InputHandler>();
    
    [field: SyncVar]
    public string Username { get; private set; }

    [field: SyncVar]
    public bool IsMangiagalli { get; private set; }

    [field: SyncVar]
    public int Score { get; private set; }
    
    [field: SyncVar]
    public int Deaths { get; set; }
    
    [field: SyncVar]
    public int Kills { get; set; }
    
    public bool IsSilent { get; private set; }
    public Vector3 LastInteractDir { get; private set; }

    public Vector3 RespawnPosition { get; set; }


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

    public override void OnStartServer()
    {
        RespawnPosition = transform.position;
    }

    #endregion

    #region CLIENT

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        var identity = gameObject.GetComponent<NetworkIdentity>();

        if (!identity.isLocalPlayer)
            OnStartOpponent();

        RespawnPosition = transform.position;
    }
    
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        LocalPlayer = this;
        
        MakeVisible();
        OnPlayerSpawned?.Invoke(LocalPlayer);
        InputHandler.OnKill += InputHandler_OnKill;
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

    [ClientRpc]
    public void RPCSetActive(bool isActive)
    {
        if (isActive)
        {
            OnPlayerRespawned?.Invoke(this);
            Opponent.MakeInvisible();
        } else
        {
            OnPlayerKilled?.Invoke(this);
            Opponent.MakeVisible();
        }
        gameObject.SetActive(isActive);
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

    private void InputHandler_OnKill(object sender, EventArgs e)
    {
        Debug.Log("Kill command received");
        Vector3 moveDir = InputHandler.GetMovementDirection();

        float interactionDistance = 2f;

        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject.TryGetComponent<Player>(out Player player))
            {
                Debug.Log("Player hit");
                RoundController.Instance.KillPlayer(player, this);
            }
        }
    }

    #endregion
}