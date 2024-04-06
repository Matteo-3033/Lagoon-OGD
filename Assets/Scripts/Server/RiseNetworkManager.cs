using System;
using MainScene;
using Mirror;
using UnityEngine;

namespace Server
{
    public class RiseNetworkManager : NetworkManager
    {
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("Client disconnected.");
            
            UIManager.Instance.ShowMenu(UIManager.Menu.Connection);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("Client connected.");
            
            UIManager.Instance.ShowMenu(UIManager.Menu.MainMenu);
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("Client connected to server.");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            Debug.Log("Client disconnected from server.");
        }

        private void OnServerInitialized()
        {
            Debug.Log("Server initialized.");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Server started.");
        }
        
        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("Server stopped.");
        }
    }
}