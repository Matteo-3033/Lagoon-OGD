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

    public Inventory Inventory => GetComponent<Inventory>();
    public PlayerPositionController PositionController => GetComponent<PlayerPositionController>();
	public PlayerRotationController RotationController => GetComponent<PlayerRotationController>();
    public TrapSelector TrapSelector => GetComponent<TrapSelector>();
    

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
        
        MakeVisible();
        OnPlayerSpawned?.Invoke(LocalPlayer);
    }
    
    private void OnStartOpponent()
    {
        Opponent = this;
        
        MakeInvisible();
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
    
    public void MakeInvisible()
    {
        gameObject.layer = LayerMask.NameToLayer("Behind-FieldOfView");
    }
    
    public void MakeVisible()
    {
        gameObject.layer = LayerMask.NameToLayer("FieldOfView");
    }

    public override void OnStopClient()
    {
        OnPlayerDespawned?.Invoke(this);
        base.OnStopClient();
    }

    #endregion
}