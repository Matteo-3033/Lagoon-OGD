using System.Linq;
using MasterServerToolkit.Extensions;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using Mirror;
using UnityEngine;
using ProfilesModule = Network.Master.ProfilesModule;
using RoomsModule = Network.Master.RoomsModule;

namespace Network.Room
{
    public class RoomServerManager : MasterServerToolkit.MasterServer.RoomServerManager
    {
        private static RiseNetworkManager NetworkManager => RiseNetworkManager.singleton;
        
        private static string OnlineScene => NetworkManager.onlineScene;
        private static string OfflineScene => NetworkManager.offlineScene;
        
        private bool Done { get; set; }
        
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
            if (Done)
                return;
            
            base.OnInitialize();
         
            Debug.Log("Starting room server");
            if (!Connection.IsConnected)
            {
                Debug.Log("Connecting to master server");
                ClientToMasterConnector.Instance.StartConnection();
            }

            ProfileFactory = RiseProfileFactory;
            
            OnBeforeRoomRegisterEvent.AddListener(_ => NetworkManager.StartRiseServer());
            OnRoomRegisterFailedEvent.AddListener(() => NetworkManager.StopRiseServer());
            
            RiseNetworkManager.OnServerStarted += OnServerStarted;
            RiseNetworkManager.OnServerStopped += DisconnectFromMaster;
            RiseNetworkManager.OnServerDisconnected += coon => OnPeerDisconnected(coon.connectionId);
        }

        private void DisconnectFromMaster()
        {
            if (!Connection.IsConnected)
                return;
            
            if (RoomController is { IsActive: true })
                RoomController.Destroy();
            
            Connection.Close();
            Done = true;
        }

        private static ObservableServerProfile RiseProfileFactory(string userId)
        {
            var profile = new ObservableServerProfile(userId);
            
            profile.Add(new ObservableInt(ProfilesModule.ScoreKey.ToUint16Hash(), 0));
            profile.Add(new ObservableInt(ProfilesModule.DeathsKey.ToUint16Hash(), 0));
            profile.Add(new ObservableInt(ProfilesModule.KillsKey.ToUint16Hash(), 0));
            
            return profile;
        }

        public override void OnServerStarted()
        {
            MatchController.Instance.OnMatchStarted += OnMatchStarted;
            base.OnServerStarted();
        }

        protected override void CreateAccessProvider(RoomAccessProviderCheck accessCheckOptions, RoomAccessProviderCallbackDelegate giveAccess)
        {
            if (MatchController.Instance != null && MatchController.Instance.Started)
                giveAccess.Invoke(null, "Match already started");
            else
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
            RoomOptions.CustomOptions.Add(RoomsModule.MatchStarted, false);
        }
        
        private void OnMatchStarted()
        {
            RoomOptions.CustomOptions.Set(RoomsModule.MatchStarted, true);
            RoomController.SaveOptions(RoomOptions);
        }
        
        public override void OnPeerDisconnected(int roomPeerId)
        {
            if (MatchController.Instance != null && TryGetRoomPlayerByRoomPeer(roomPeerId, out var player))
                MatchController.Instance.OnPlayerDisconnected(player.Username);
            
            MstTimer.WaitForSeconds(Mst.Server.Profiles.ProfileUpdatesInterval, () => base.OnPeerDisconnected(roomPeerId));
        }

        protected override void OnPlayerLeftRoom(RoomPlayer player)
        {
            if (Connection.IsConnected)
                base.OnPlayerLeftRoom(player);
        }

        public RoomPlayer GetPlayerProfile(NetworkConnection conn)
        {
            return Players.FirstOrDefault(p => p.RoomPeerId == conn.connectionId);
        }

        public string Username(NetworkConnection conn)
        {
            return GetPlayerProfile(conn)?.Username;
        }
    }
}