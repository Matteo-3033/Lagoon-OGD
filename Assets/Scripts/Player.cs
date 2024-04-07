using Mirror;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkMatch))]
public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; private set;  }
    
    [SyncVar] public string username;
    [SyncVar] public MatchController match;

    private NetworkMatch networkMatch;

    private void Awake()
    {
        networkMatch = gameObject.GetComponent<NetworkMatch>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
     
        if (isLocalPlayer)
            LocalPlayer = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        username = connectionToClient.Username();
    }
    
    [ServerCallback]
    public void SetMatch(MatchController matchController)
    {
        // Run on server
        match = matchController;
        networkMatch.matchId = matchController.matchId.ToGuid();
    }

    [TargetRpc]
    public void LoadMatch()
    {
        // Run on client
        Debug.Log ("Beginning match");
        SceneManager.LoadScene(Utils.Scenes.Round, LoadSceneMode.Additive);
    }
}
