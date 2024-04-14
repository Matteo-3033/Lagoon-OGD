using MasterServerToolkit.Extensions;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;

namespace Network.Master
{
    public class ProfilesModule: MasterServerToolkit.MasterServer.ProfilesModule
    {
        public const string ScoreKey = "score";
        public const string DeathsKey = "deaths";
        public const string KillsKey = "kills";

        public override void Initialize(IServer server)
        {
            base.Initialize(server);

            ProfileFactory = RiseProfileFactory;
        }

        private static ObservableServerProfile RiseProfileFactory(string userId, IPeer clientPeer)
        {
            var profile = new ObservableServerProfile(userId, clientPeer);
            
            profile.Add(new ObservableInt(ScoreKey.ToUint16Hash(), 0));
            profile.Add(new ObservableInt(DeathsKey.ToUint16Hash(), 0));
            profile.Add(new ObservableInt(KillsKey.ToUint16Hash(), 0));
            
            return profile;
        }
    }

    public static class RoomPlayerExtensions
    {
        public static ObservableInt Score(this RoomPlayer player)
        {
            return player.Profile.Get<ObservableInt>(ProfilesModule.ScoreKey);
        }
        
        public static ObservableInt Deaths(this RoomPlayer player)
        {
            return player.Profile.Get<ObservableInt>(ProfilesModule.DeathsKey);
        }
        
        public static ObservableInt Kills(this RoomPlayer player)
        {
            return player.Profile.Get<ObservableInt>(ProfilesModule.KillsKey);
        }
    }
}