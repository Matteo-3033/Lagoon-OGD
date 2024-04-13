using System;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; private set;  }
    public static Player Opponent { get; private set;  }

    [SyncVar] private string username;
    [SyncVar] private bool isMangiagalli;
    
    public string Username => username;
    public bool IsMangiagalli => isMangiagalli;

    public int Score => 0;
    
    public static event Action<bool> OnPlayerSpawned;
    public static event Action<bool> OnPlayerDespawned; 
    

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
        OnPlayerSpawned?.Invoke(true);
    }
    
    private void OnStartOpponent()
    {
        Opponent = this;
        
        gameObject.layer = LayerMask.NameToLayer("Behind-FieldOfView");
        OnPlayerSpawned?.Invoke(false);
    }

    #region CLIENT
    
    public void EnableMovement()
    {
        GetComponent<PlayerPositionController>().SetEnabled(true);    
    }

    public void Init(string username, bool isMangiagalli)
    {
        this.username = username;
        this.isMangiagalli = isMangiagalli;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        OnPlayerDespawned?.Invoke(isLocalPlayer);
    }

    #endregion
}