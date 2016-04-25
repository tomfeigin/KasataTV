using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using PcapDotNet.Packets.Http;
using Newtonsoft.Json;
using PcapDotNet.Packets.Ethernet;
using System.IO.Compression;
using System.IO;

namespace CastersPOC
{
    class NetworkFilter
    {
        public static void Begin()
        {
            // Retrieve the device list from the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                Console.WriteLine("No interfaces found! Make sure WinPcap is installed.");
                return;
            }

            // Print the list
            for (int i = 0; i != allDevices.Count; ++i)
            {
                LivePacketDevice device = allDevices[i];
                Console.Write((i + 1) + ". " + device.Name);
                if (device.Description != null)
                    Console.WriteLine(" (" + device.Description + ")");
                else
                    Console.WriteLine(" (No description available)");
            }

            int deviceIndex = 0;
            do
            {
                Console.WriteLine("Enter the interface number (1-" + allDevices.Count + "):");
                string deviceIndexString = "1";// Console.ReadLine();
                if (!int.TryParse(deviceIndexString, out deviceIndex) ||
                    deviceIndex < 1 || deviceIndex > allDevices.Count)
                {
                    deviceIndex = 0;
                }
            } while (deviceIndex == 0);

            // Take the selected adapter
            PacketDevice selectedDevice = allDevices[deviceIndex - 1];

            // Open the device
            using (PacketCommunicator communicator =
                selectedDevice.Open(15046,                                  // portion of the packet to capture
                // 65536 guarantees that the whole packet will be captured on all the link layers
                                    PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                    1000))                                  // read timeout
            {
                // Check the link layer. We support only Ethernet for simplicity.
                if (communicator.DataLink.Kind != DataLinkKind.Ethernet)
                {
                    Console.WriteLine("This program works only on Ethernet networks.");
                    return;
                }

                // Compile the filter
                using (BerkeleyPacketFilter filter = communicator.CreateFilter("ip and tcp"))
                {
                    // Set the filter
                    communicator.SetFilter(filter);
                }

                Console.WriteLine("Listening on " + selectedDevice.Description + "...");

                // start the capture
                communicator.ReceivePackets(0, PacketHandler);
            }
        }

        private static string Unzip(byte[] bytes) 
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        // Callback function invoked by libpcap for every incoming packet
        private static void PacketHandler(Packet packet)
        {
            // print timestamp and length of the packet
            IpV4Datagram ip = packet.Ethernet.IpV4;
            String source = ip.Source.ToString();

            // Chat service IP.
            if (source != "178.79.171.171")
            {
                return;
            }

            // get original layers
            var ethernet = (EthernetLayer)packet.Ethernet.ExtractLayer();
            var ipV4 = (IpV4Layer)packet.Ethernet.IpV4.ExtractLayer();
            var tcp = (TcpLayer)packet.Ethernet.IpV4.Tcp.ExtractLayer();
            var http = (HttpLayer)packet.Ethernet.IpV4.Tcp.Http.ExtractLayer();
            string ethString = packet.Ethernet.BytesSequenceToHexadecimalString("-");
            string ipv4String = packet.Ethernet.IpV4.BytesSequenceToHexadecimalString("-");
            string tcpString = packet.Ethernet.IpV4.Tcp.BytesSequenceToHexadecimalString("-");
            string transportString = packet.Ethernet.Ip.Transport.BytesSequenceToHexadecimalString("-");
            string httpString = packet.Ethernet.Ip.Tcp.Http.BytesSequenceToHexadecimalString("-");
            if (http.Body == null)
            {
                return;
            }

            var time = packet.Timestamp;
            string transportHead = packet.Ethernet.Ip.Transport.Decode(Encoding.UTF8);
            string transportData = packet.Ethernet.Ip.Transport.Payload.Decode(Encoding.UTF8);
            string htmlHead = packet.Ethernet.Ip.Tcp.Http.Decode(Encoding.UTF8);
            string htmlData = packet.Ethernet.Ip.Tcp.Http.Body.Decode(Encoding.UTF8);

            byte[] bodyStream = packet.Ethernet.Ip.Tcp.Http.Body.ToArray();
            byte[] decompressed = Decompress(bodyStream);
            byte[] httpBody = http.Body.ToArray();
            string result = Unzip(httpBody);
            
            // extract the data
            PayloadLayer payload = (PayloadLayer)packet.Ethernet.IpV4.Tcp.Payload.ExtractLayer();
            var totalLength = payload.Length;
            var buffer = new byte[totalLength];
            
            ////payload.Write(buffer, 1, totalLength, ipV4, null);
            Datagram data = payload.Data;
            string someSHit = data.BytesSequenceToHexadecimalString("-");
            string jsonStr = data.Decode(Encoding.ASCII);
            object jsonShit = JsonConvert.DeserializeObject<Dictionary<String, Object>>(jsonStr);

            System.Windows.Forms.MessageBox.Show("I'm here!!!");
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
      }
}
