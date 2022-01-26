using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace FragmentNetslumServer.Helpers
{

    /// <summary>
    /// Some IP address related helper functions
    /// </summary>
    public static class IPAddressHelpers
    {

        /// <summary>
        /// Defines, as a global constant, the permanent loopback IP address (aka LOCALHOST)
        /// </summary>
        public const string LOOPBACK_IP_ADDRESS = "127.0.0.1";


        /// <summary> 
        /// Selects an IPv4 address to use instead of 127.0.0.1
        /// </summary> 
        public static string GetLocalIPAddress()
        {

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection) 
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network 
                IPInterfaceProperties properties = network.GetIPProperties();

                // Each network interface may have multiple IP addresses 
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now 
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1) 
                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    return address.Address.ToString();
                }
            }

            return "0.0.0.0";
            
        }


        public static string GetLocalIPAddress2()
        {
            var addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress a in addresses)
            {
                if (a.AddressFamily == AddressFamily.InterNetwork)
                {
                    return a.ToString();
                }
            }

            return "0.0.0.0";

        }

    }
}
