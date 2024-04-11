using MasterServerToolkit.Networking;

namespace Network
{
    public class SpawnerBehaviour: MasterServerToolkit.MasterServer.SpawnerBehaviour
    {
        protected override void OnConnectedToMasterEventHandler(IClientSocket client)
        {
            StartSpawner();
        }
    }
}