using MasterServerToolkit.Extensions;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.Utils;
using Mirror;
using UnityEngine;
using ValidateRoomAccessRequestMessage = Network.Messages.ValidateRoomAccessRequestMessage;
using ValidateRoomAccessResultMessage = Network.Messages.ValidateRoomAccessResultMessage;

namespace Network.Client
{
    public class RoomClientManager : MasterServerToolkit.MasterServer.RoomClientManager
    {
        protected override void Awake()
        {
            if (!RiseNetworkManager.IsClient)
            {
                Destroy(gameObject);
                return;
            }
                
            base.Awake();
        }
        
        private static RiseNetworkManager NetworkManager => RiseNetworkManager.singleton;
        
        private static string OnlineScene => NetworkManager.onlineScene;
        private static string OfflineScene => NetworkManager.offlineScene;
        
        protected override void Start()
        {
            base.Start();
            
            RiseNetworkManager.OnClientConnected += NetworkManager_OnConnectedEvent;
            RiseNetworkManager.OnClientDisconnected += NetworkManager_OnDisconnectedEvent;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RiseNetworkManager.OnClientConnected -= NetworkManager_OnConnectedEvent;
            RiseNetworkManager.OnClientDisconnected -= NetworkManager_OnDisconnectedEvent;
        }

        private void OnApplicationQuit()
        {
            StartDisconnection();
        }

        protected override void StartConnection(RoomAccessPacket access)
        {
            Debug.Log("Start connection to room server");
            if (NetworkClient.isConnected)
                return;
            
            NetworkManager.SetAddress(access.RoomIp);
            NetworkManager.SetPort(access.RoomPort);

            Debug.Log($"Start joining a room at {access.RoomIp}:{access.RoomPort}. Scene: {access.SceneName}");
            Debug.Log($"Custom info: {access.CustomOptions}");
            
            NetworkManager.StartClient();
        }
        
        protected override void StartDisconnection()
        {
            NetworkManager.StopClient();
        }

        private void NetworkManager_OnConnectedEvent(NetworkConnection conn)
        { 
            Debug.Log($"Waiting for access data. Timeout in {roomConnectionTimeout} sec.");

            MstTimer.WaitWhile(() => !Mst.Client.Rooms.HasAccess, isSuccess =>
            {
                if (!isSuccess)
                {
                    Debug.LogError("Room connection timeout");
                    Disconnect();
                    return;
                }

                Debug.Log($"Validating access to room server with token [{Mst.Client.Rooms.ReceivedAccess.Token}]");

                // Register listener for access validation message from mirror room server
                NetworkClient.RegisterHandler<ValidateRoomAccessResultMessage>(ValidateRoomAccessResultHandler, false);

                // Send validation message to room server
                conn.Send(new ValidateRoomAccessRequestMessage
                {
                    Token = Mst.Client.Rooms.ReceivedAccess.Token
                });

                Debug.Log($"You have joined the room at {Mst.Client.Rooms.ReceivedAccess.RoomIp}:{Mst.Client.Rooms.ReceivedAccess.RoomPort}");
            }, roomConnectionTimeout);
        }

        private void NetworkManager_OnDisconnectedEvent(NetworkConnection conn)
        {
            Debug.Log("You have just been disconnected from the server");

            NetworkClient.UnregisterHandler<ValidateRoomAccessResultMessage>();
            LoadOfflineScene();
        }

        private void ValidateRoomAccessResultHandler(ValidateRoomAccessResultMessage msg)
        {
            if (msg.Status != ResponseStatus.Success)
            {
                Debug.LogError(msg.Error);
                return;
            }

            Debug.Log("Access to server room is successfully validated");
            
            if (!NetworkClient.ready)
                NetworkClient.Ready();
        }
        
        protected override void LoadOnlineScene()
        {
            Debug.Log($"Loading online scene {OnlineScene}".ToGreen());

            ScenesLoader.LoadSceneByName(OnlineScene, (progressValue) =>
            {
                Mst.Events.Invoke(MstEventKeys.showLoadingInfo, $"Loading scene {Mathf.RoundToInt(progressValue * 100f)}% Please wait!");
            },
            () =>
            {
                if (!NetworkClient.ready)
                    NetworkClient.Ready();
            });
        }
        
        protected override void LoadOfflineScene()
        {
            if (!string.IsNullOrEmpty(OfflineScene))
            {
                ScenesLoader.LoadSceneByName(OfflineScene, (progressValue) =>
                {
                    Mst.Events.Invoke(MstEventKeys.showLoadingInfo, $"Loading scene {Mathf.RoundToInt(progressValue * 100f)}% ... Please wait!");
                }, null);
            }
        }

        // Framework method not used
        protected override void StartGame()
        {
        }
    }
}