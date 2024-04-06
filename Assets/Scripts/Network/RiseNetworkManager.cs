using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Server
{
    public class RiseNetworkManager : NetworkManager
    {
        public new static RiseNetworkManager singleton => NetworkManager.singleton as RiseNetworkManager;

        public static event Action<NetworkConnectionToClient> OnClientConnected;
        public static event Action OnClientDisconnected;
        public static event Action OnServerStopped;

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("Client disconnected.");
            
            SceneManager.LoadScene(Utils.Scenes.Connection);
            OnClientDisconnected?.Invoke();
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("Client connected.");
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("Client connected to server.");
            
            if (numPlayers >= maxConnections)
            {
                conn.Disconnect();
                return;
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Debug.Log("Client disconnected from server.");
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
            
            OnClientConnected?.Invoke(conn);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Server started.");
            
            spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
        }

        public override void OnStartClient()
        {
            var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
        }
    }
}