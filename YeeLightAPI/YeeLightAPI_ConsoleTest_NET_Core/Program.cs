using System;
using System.Collections.Generic;
using YeeLightAPI;
using YeeLightAPI.YeeLightDeviceLocator;

namespace YeeLightAPI_ConsoleTest_NET_Core
{
    class Program
    {
        static void Main()
        {
            int SSDPTimeOut = 10000;
            int SSDPRequestCount = 1;

            Console.WriteLine("Beware that these devices usually have rate limits on SSDP requests with very long wait times. You might have to power cycle your device to zero the rate limit delay.");
            Console.WriteLine("Press any key start a SSDP search request...");
            Console.ReadKey();

            Console.WriteLine($"Sending SSDP search request with {SSDPTimeOut}ms timeout and request count of {SSDPRequestCount} ...");
            Console.WriteLine();

            List<YeeLightDevice> devices = DeviceLocator.DiscoverDevices(SSDPTimeOut, SSDPRequestCount);
            if (devices.Count > 0)
            {
                Console.WriteLine($"Found {devices.Count} device(s): ");
                foreach (YeeLightDevice device in devices)
                {
                    var (ipAddress, port) = device.GetLightIPAddressAndPort();
                    Console.WriteLine($"IP: {ipAddress} | Port: {port}");
                }
            }
            else
            {
                Console.WriteLine("No devices have been found.");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
