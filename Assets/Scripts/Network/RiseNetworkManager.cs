using System;
using System.Collections;
using System.Linq;
using kcp2k;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;
using ProfilesModule = Network.Master.ProfilesModule;
using RoomServerManager = Network.Room.RoomServerManager;
using ValidateRoomAccessRequestMessage = Network.Messages.ValidateRoomAccessRequestMessage;
using ValidateRoomAccessResultMessage = Network.Messages.ValidateRoomAccessResultMessage;

namespace Network
{
    public class RiseNetworkManager : NetworkManager
    {
        
        [SerializeField] private RoomServerManager roomServerManager;
        [SerializeField] private GameObject matchControllerPrefab;
        
        public new static RiseNetworkManager singleton => NetworkManager.singleton as RiseNetworkManager;

        // Called on client when client is ready
        public static event Action<NetworkConnection> OnClientConnected;
        
        // Called on client when client disconnects
        public static event Action<NetworkConnection> OnClientDisconnected;
        
        // Called on server when server starts
        public static event Action OnServerStarted;
        
        // Called on server when server stops
        public static event Action OnServerStopped;
        
        // Called on server when client is ready
        public static event Action<NetworkConnectionToClient, string> OnServerReadied;
        
        // Called on server when a client player object is created
        public static event Action<NetworkConnectionToClient, string> OnServerPlayerAdded;
        
        // Called on server when client disconnects
        public static event Action<NetworkConnectionToClient> OnServerDisconnected;

        public static bool IsClient => !Mst.Server.Spawners.IsSpawnedProccess && !Application.isBatchMode;
        public static RoomOptions RoomOptions => singleton.roomServerManager.RoomOptions;


        public override void Awake()
        {
            if (NetworkManager.singleton != null)
                return;
            
            base.Awake();
            
            headlessStartMode = HeadlessStartOptions.DoNothing;
        }
        
        public void StartRiseServer()
        {
            maxConnections = roomServerManager.RoomOptions.MaxConnections;
            SetAddress(roomServerManager.RoomOptions.RoomIp);
            SetPort(roomServerManager.RoomOptions.RoomPort);

            Debug.Log($"Starting Room Server: {networkAddress}:{roomServerManager.RoomOptions.RoomPort}");
            Debug.Log($"Online Scene: {onlineScene}");

            #if UNITY_EDITOR
                StartHost();
            #else
                StartServer();
            #endif
        }
        
        public void StopRiseServer()
        {
            #if UNITY_EDITOR
                StopHost();
            #else
                StopServer();
            #endif
            StartCoroutine(DoStopRoomServer());
        }

        private IEnumerator DoStopRoomServer()
        {
            yield return new WaitForSeconds(1);
            Utils.Runtime.Quit();
        }

        #region CLIENT

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("Client disconnected");
            
            OnClientDisconnected?.Invoke(NetworkClient.connection);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("Client connected");
            
            OnClientConnected?.Invoke(NetworkClient.connection);
        }

        #endregion

        #region SERVER

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("Client connected to rise server");
            
            if (numPlayers >= maxConnections)
                conn.Disconnect();
        }
        
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            StartCoroutine(DoOnServerDisconnect(conn));
        }

        private IEnumerator DoOnServerDisconnect(NetworkConnectionToClient conn)
        {
            OnServerDisconnected?.Invoke(conn);
            yield return null;
            
            base.OnServerDisconnect(conn);
            
            Debug.Log("Client disconnected from rise server");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Rise server started");
            
            NetworkServer.RegisterHandler<ValidateRoomAccessRequestMessage>(ValidateRoomAccessRequestHandler, false);
            
            var matchController = Instantiate(matchControllerPrefab);
            NetworkServer.Spawn(matchController);
            
            OnServerStarted?.Invoke();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("Rise server stopped");
            
            NetworkServer.UnregisterHandler<ValidateRoomAccessRequestMessage>();
            
            OnServerStopped?.Invoke();
        }
        
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            Debug.Log("Client ready");

            var username = roomServerManager.Username(conn);
            OnServerReadied?.Invoke(conn, username);
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var startPos = GetStartPosition();
            var player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            StartCoroutine(DoOnServerAddPlayer(conn, player));
        }
        
        private IEnumerator DoOnServerAddPlayer(NetworkConnectionToClient conn, GameObject player)
        {
            // Wait for player object to be actually created
            yield return null;
            
            var profile = roomServerManager.GetPlayerProfile(conn);
            
            player.name = profile?.Username ?? $"Player {conn.connectionId}";
            player.GetComponent<Player>().Init(profile, roomServerManager.Players.Count() == 1);
            
            NetworkServer.AddPlayerForConnection(conn, player);
            
            Debug.Log("Player object created");
            
            OnServerPlayerAdded?.Invoke(conn, profile?.Username);
        }

        private void ValidateRoomAccessRequestHandler(NetworkConnection conn, ValidateRoomAccessRequestMessage mess)
        {
            roomServerManager.ValidateRoomAccess(conn.connectionId, mess.Token, (isSuccess, error) =>
            {
                try
                {
                    if (!isSuccess)
                    {
                        Debug.LogError(error);

                        conn.Send(new ValidateRoomAccessResultMessage()
                        {
                            Error = error,
                            Status = ResponseStatus.Failed
                        });

                        MstTimer.WaitForSeconds(1f, conn.Disconnect);
                        return;
                    }

                    conn.Send(new ValidateRoomAccessResultMessage
                    {
                        Error = string.Empty,
                        Status = ResponseStatus.Success
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    conn.Send(new ValidateRoomAccessResultMessage
                    {
                        Error = e.Message,
                        Status = ResponseStatus.Error
                    });

                    MstTimer.WaitForSeconds(1f, conn.Disconnect);
                }
            });
        }
        
        #endregion


        public void SetAddress(string address)
        {
            networkAddress = address;
        }
        
        public void SetPort(int port)
        {
            switch (Transport.active)
            {
                case KcpTransport kcpTransport:
                    kcpTransport.Port = (ushort)port;
                    break;
                case TelepathyTransport telepathyTransport:
                    telepathyTransport.port = (ushort)port;
                    break;
                case SimpleWebTransport swTransport:
                    swTransport.port = (ushort)port;
                    break;
            }
        }
    }
}