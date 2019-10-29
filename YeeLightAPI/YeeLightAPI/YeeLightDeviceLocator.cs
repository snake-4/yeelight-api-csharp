using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace YeeLightAPI
{
    namespace YeeLightDeviceLocator
    {
        public static class DeviceLocator
        {
            private const string SSDP_locationKey = "Location: ";
            private const string SSDP_searchMessage = "M-SEARCH * HTTP/1.1\r\nMAN: \"ssdp:discover\"\r\nST: wifi_bulb\r\n"; //Yeelight SSDP search message
            private const string multiCastAddress = "239.255.255.250"; //YeeLight multicast address
            private const int SSDP_port = 1982; //YeeLight SSDP port
            private const int SSDP_timeOut = 1000; //Default time in microseconds to wait for SSDP packet

            public static List<YeeLightDevice> DiscoverDevices(int SSDP_receiveTimeOut = SSDP_timeOut, int pingCount = 1)
            {
                Dictionary<IPAddress, YeeLightDevice> devices = new Dictionary<IPAddress, YeeLightDevice>();

                using (UdpClient socket = new UdpClient())
                {
                    IPAddress multicastAddress = IPAddress.Parse(multiCastAddress);
                    IPEndPoint multicastEndpoint = new IPEndPoint(multicastAddress, SSDP_port);
                    socket.JoinMulticastGroup(multicastAddress);

                    socket.Client.ReceiveTimeout = SSDP_timeOut;

                    byte[] buffer = Encoding.ASCII.GetBytes(SSDP_searchMessage);

                    for (int i = 0; i < pingCount; i++)
                    {
                        socket.Send(buffer, buffer.Length, multicastEndpoint);

                        while (true)
                        {
                            try
                            {
                                IPEndPoint sourceEndPoint = null;
                                byte[] message = socket.Receive(ref sourceEndPoint);
                                IPAddress deviceIp = sourceEndPoint.Address;

                                lock (devices)
                                {
                                    if (devices.ContainsKey(deviceIp))
                                    {
                                        continue;
                                    }
                                    YeeLightDevice device = GetDeviceFromSsdpMessage(Encoding.ASCII.GetString(message));
                                    devices.Add(deviceIp, device);
                                }
                            }
                            catch
                            {
                                break;
                            }
                        }

                    }
                }

                return devices.Select(n => n.Value).ToList();
            }

            private static YeeLightDevice GetDeviceFromUri(Uri deviceUri)
            {
                ushort port = YeeLightConstants.Constants.DefaultCommandPort;
                if (!deviceUri.IsDefaultPort)
                {
                    port = (ushort)deviceUri.Port;
                }

                return new YeeLightDevice(deviceUri.Host, port);
            }

            private static YeeLightDevice GetDeviceFromSsdpMessage(string ssdpMessage)
            {
                if (ssdpMessage != null)
                {
                    string[] split = ssdpMessage.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string part in split)
                    {
                        if (part.StartsWith(SSDP_locationKey))
                        {
                            string valueOfPair = part.Substring(SSDP_locationKey.Length);
                            return GetDeviceFromUri(new Uri(valueOfPair));
                        }
                    }
                }

                return null;
            }
        }
    }
}
