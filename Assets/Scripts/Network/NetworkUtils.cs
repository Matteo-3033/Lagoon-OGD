using System.Linq;
using System.Net;

namespace Network
{
    public static class NetworkUtils
    {
        private static string _machineIp;
        
        public static string GetMachineIp()
        {
            if (_machineIp == null)
                _machineIp =  Dns.GetHostEntry(System.Net.Dns.GetHostName())
                .AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
            
            return _machineIp;
        }
    }
}