using System;
using MasterServerToolkit.MasterServer;
using Mirror;
using Network.Master;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;
    
    public static Player LocalPlayer { get; private set;  }
    public static Player Opponent { get; private set;  }
    
    public Inventory Inventory { get; private set; }
	public PlayerPositionController PositionController { get; private set; }
	public PlayerRotationController RotationController { get; private set; }

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

    private void Awake()
    {
        Inventory = GetComponent<Inventory>();
		PositionController = GetComponent<PlayerPositionController>();
		RotationController = GetComponent<PlayerRotationController>();
    }

    #region SERVER
    
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
        
        gameObject.layer = LayerMask.NameToLayer("FieldOfView");
        OnPlayerSpawned?.Invoke(LocalPlayer);
    }
    
    private void OnStartOpponent()
    {
        Opponent = this;
        
        gameObject.layer = LayerMask.NameToLayer("Behind-FieldOfView");
        OnPlayerSpawned?.Invoke(Opponent);
    }
    
    public void Init(RoomPlayer profile, bool isMangiagalli)
    {
        Username = profile.Username;
        IsMangiagalli = isMangiagalli;
        Score = profile.Score().Value;
        Deaths = profile.Deaths().Value;
        Kills = profile.Kills().Value;
    }
    
    #endregion

    #region CLIENT
    
    public void EnableMovement(bool enable)
    {
        PositionController.SetEnabled(enable);    
    }

    public override void OnStopClient()
    {
        OnPlayerDespawned?.Invoke(this);
        base.OnStopClient();
    }

    #endregion
}