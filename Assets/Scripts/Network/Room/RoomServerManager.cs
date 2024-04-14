using System.Linq;
using MasterServerToolkit.Extensions;
using MasterServerToolkit.MasterServer;
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

            ProfileFactory = RiseProfileFactory;
            
            OnBeforeRoomRegisterEvent.AddListener(_ => NetworkManager.StartRiseServer());
            OnRoomRegisterFailedEvent.AddListener(() => NetworkManager.StopRiseServer());
            
            RiseNetworkManager.OnServerStarted += OnServerStarted;
            RiseNetworkManager.OnServerStopped += OnServerStopped;
            RiseNetworkManager.OnServerDisconnected += coon => OnPeerDisconnected(coon.connectionId);
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
            MatchController.Instance.OnMatchStart += OnMatchStarted;
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