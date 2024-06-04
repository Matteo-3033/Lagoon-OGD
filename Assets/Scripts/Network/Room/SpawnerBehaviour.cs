using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;

namespace Network
{
    public class SpawnerBehaviour: MasterServerToolkit.MasterServer.SpawnerBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            ClientToMasterConnector.Instance.SetIpAddress(NetworkUtils.GetMachineIp());
            machineIp = NetworkUtils.GetMachineIp();
            ClientToMasterConnector.Instance.StartConnection();
        }

        protected override void OnConnectedToMasterEventHandler(IClientSocket client)
        {
            StartSpawner();
        }
    }
}