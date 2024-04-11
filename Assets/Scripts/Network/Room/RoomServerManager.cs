using MasterServerToolkit.MasterServer;
using UnityEngine;

namespace Network.Room
{
    public class RoomServerManager : MasterServerToolkit.MasterServer.RoomServerManager
    {
        
        private static RiseNetworkManager NetworkManager => RiseNetworkManager.singleton;
        
        private static string OnlineScene => NetworkManager.onlineScene;
        private static string OfflineScene => NetworkManager.offlineScene;
        
        protected override void Awake()
        {
            if (RiseNetworkManager.IsClient)
            {
                Destroy(gameObject);
                return;
            }
                
            base.Awake();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            OnBeforeRoomRegisterEvent.AddListener(_ => NetworkManager.StartRiseServer());
            OnRoomRegisterFailedEvent.AddListener(() => NetworkManager.StopRiseServer());
            RiseNetworkManager.OnServerStarted += OnServerStarted;
            RiseNetworkManager.OnServerStopped += OnServerStopped;
            RiseNetworkManager.OnServerDisconnected += coon => OnPeerDisconnected(coon.connectionId);
        }

        protected override void CreateAccessProvider(RoomAccessProviderCheck accessCheckOptions, RoomAccessProviderCallbackDelegate giveAccess)
        {
            giveAccess.Invoke(new RoomAccessPacket
            {
                RoomId = RoomController.RoomId,
                RoomIp = RoomController.Options.RoomIp,
                RoomPort = RoomController.Options.RoomPort,
                RoomMaxConnections = RoomController.Options.MaxConnections,
                CustomOptions = RoomController.Options.CustomOptions,
                Token = Mst.Helper.CreateGuidString(),
                SceneName = OnlineScene
            }, null);
        }
        
        protected override void BeforeRoomRegistering()
        {
            base.BeforeRoomRegistering();

            Debug.Log("Registering room");
            
            RoomOptions.IsPublic = true;
            RoomOptions.MaxConnections = 2;
            RoomOptions.Password = string.Empty;
            RoomOptions.Region = string.Empty;
        }
    }
}