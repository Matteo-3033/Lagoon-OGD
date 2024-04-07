using System;
using System.Collections;
using Mirror;
using UnityEngine;

namespace Network
{
    public class RiseNetworkManager : NetworkManager
    {
        [SerializeField] private MatchMaker matchMakerPrefab;
        public Authenticator Authenticator { get; private set; }
        
        public new static RiseNetworkManager singleton => NetworkManager.singleton as RiseNetworkManager;

        // Called on client when client is ready
        public static event Action OnClientConnected;
        
        // Called on client when client disconnects
        public static event Action OnClientDisconnected;
        
        // Called on server when server stops
        public static event Action OnServerStopped;
        
        // Called on server when client is ready
        public static event Action<NetworkConnectionToClient> OnServerReadied;
        
        // Called on server when client disconnects
        public static event Action<NetworkConnectionToClient> OnServerDisconnected;

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
            StartCoroutine(DoServerDisconnect(conn));
        }

        private IEnumerator DoServerDisconnect(NetworkConnectionToClient conn)
        {
            OnServerDisconnected?.Invoke(conn);
            yield return null;
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
            OnServerReadied?.Invoke(conn);
        }
    }
}