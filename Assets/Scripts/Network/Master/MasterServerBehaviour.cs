using UnityEngine;

namespace Network.Master
{
    public class MasterServerBehaviour: MasterServerToolkit.MasterServer.MasterServerBehaviour
    {
        protected override void Awake()
        {
            var address = NetworkUtils.GetMachineIp();
            serverIp = address;
            Debug.Log("\nMachine address: " + address + "\n");
            
            base.Awake();
        }
    }
}