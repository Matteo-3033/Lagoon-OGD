using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class RiseNetworkManager : NetworkManager
    {
        [SerializeField] private MatchMaker matchMakerPrefab;
        public Authenticator Authenticator { get; private set; }
        
        public new static RiseNetworkManager singleton => NetworkManager.singleton as RiseNetworkManager;

        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;
        public static event Action OnServerStopped;

        public override void Awake()
        {
            base.Awake();
            Authenticator = gameObject.GetComponent<Authenticator>();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("Client disconnected.");
            
            OnClientDisconnected?.Invoke();
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("Client connected.");
            
            OnClientConnected?.Invoke();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("Client connected to server.");
            
            if (numPlayers >= maxConnections)
                conn.Disconnect();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (conn.identity)
            {
                var player = conn.identity.GetComponent<Player>();
                MatchMaker.Instance.DisconnectPlayer(player);
                Authenticator.OnPlayerDisconnected(player);
            }
                
            base.OnServerDisconnect(conn);
            Debug.Log("Client disconnected from server.");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Server started.");
            
            Instantiate(matchMakerPrefab);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("Server stopped.");
            
            OnServerStopped?.Invoke();
        }
        
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            Debug.Log("Client ready");
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            Debug.Log("Player added");
        }
    }
}