using System.Linq;
using System.Net;
using UnityEngine;

namespace Network.Master
{
    public class MasterServerBehaviour: MasterServerToolkit.MasterServer.MasterServerBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            
            var addresses = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.Where(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(a => a.ToString());
            
            Debug.Log("Addresses:");
            foreach (var address in addresses)
                Debug.Log("<color=blue>" + address + "</color>");
        }
    }
}