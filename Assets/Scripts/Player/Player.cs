using System;
using Interaction;
using MasterServerToolkit.MasterServer;
using Mirror;
using Network.Master;
using UnityEngine;
using Utils;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;
    public event EventHandler<Vector3> OnPositionChanged;
    
    public static Player LocalPlayer { get; private set;  }
    public static Player Opponent { get; private set;  }

    public Inventory Inventory => GetComponent<Inventory>();
    public PlayerPositionController PositionController => GetComponent<PlayerPositionController>();
	public PlayerRotationController RotationController => GetComponent<PlayerRotationController>();
    public TrapSelector TrapSelector => GetComponent<TrapSelector>();
    public Interactor Interactor => GetComponent<Interactor>();
    public FieldOfView FieldOfView => GetComponentInChildren<FieldOfView>();
    

    [field: SyncVar]
    public string Username { get; private set; }

    [field: SyncVar]
    public bool IsMangiagalli { get; private set; }

    [field: SyncVar]
    public int Score { get; private set; }
    
    [field: SyncVar]
    public int Deaths { get; private set; }
    
    [field: SyncVar]
    public int Kills { get; private set; }
    
    public bool IsSilent { get; private set; }
    

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
    
    #endregion
}