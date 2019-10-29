using System;
using System.Collections.Generic;
using YeeLightAPI;
using YeeLightAPI.YeeLightDeviceLocator;

namespace YeeLightAPI_ConsoleTest_NET_Core
{
    class Program
    {
        static void Main(string[] args)
        {
            int SSDPTimeOut = 1000;
            int SSDPRequestCount = 1;

            Console.WriteLine($"Sending SSDP search request with {SSDPTimeOut}ms timeout and request count of {SSDPRequestCount} ...");
            Console.WriteLine();

            List<YeeLightDevice> devices = DeviceLocator.DiscoverDevices(SSDPTimeOut, SSDPRequestCount);
            if (devices.Count > 0)
            {
                Console.WriteLine($"Found {devices.Count} device(s): ");
                foreach (YeeLightDevice device in devices)
                {
                    var ipAndPort = device.GetLightIPAddressAndPort();
                    Console.WriteLine($"IP: {ipAndPort.ipAddress} | Port: {ipAndPort.port}");
                }
            }
            else
            {
                Console.WriteLine("No devices has been found.");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
