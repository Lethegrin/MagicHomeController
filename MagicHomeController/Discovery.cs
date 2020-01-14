using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicHomeConsoleApp
{
    public class Discovery
    {

        private
        const int DISCOVERY_PORT = 48899; //Port that we listen on
        byte[] MAGIC_UDP_PACKET = Encoding.ASCII.GetBytes("HF-A11ASSISTHREAD"); //Encode magic UDP packet


        public List<Bulb> bulbList = new List<Bulb>(); //list of all bulb objects
        private CancellationTokenSource m_cancelScanSource; //cancelation token to cancel our scan method

        public async Task<List<Bulb>> Scan(int millisecondsTimeout = 2000, int maxRetries = 2)
        {

            using UdpClient discovery_client = new UdpClient(); //Create UDP Client for discovery broadcast
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT);



            int scanRetryCount = 0;

            //repeat the scan if we didn't hear back from any bulbs, but only if scanRetryCount doesn't exceed 'maxRetries'
            while (bulbList.Count == 0 || scanRetryCount <= maxRetries)
            {

                m_cancelScanSource = new CancellationTokenSource(millisecondsTimeout); //creae the cancellation token         

                Console.WriteLine("sending magic packet");
                discovery_client.Send(MAGIC_UDP_PACKET, MAGIC_UDP_PACKET.Length, ip); //Send magic packet to get controllers to announce themselves

                //Listen for their return packets
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, DISCOVERY_PORT);

                while (true)
                {
                    //Hack in a way to allow a CancellationToken for ReceiveAsync
                    //Based heavily on https://stackoverflow.com/questions/19404199/how-to-to-make-udpclient-receiveasync-cancelable
                    var receive_task = discovery_client.ReceiveAsync();
                    var tcs = new TaskCompletionSource<bool>();

                    using (m_cancelScanSource.Token.Register(s => tcs.TrySetResult(true), null))
                    {
                        if (await Task.WhenAny(receive_task, tcs.Task) == receive_task) //if the cancellation token isn't true continue, else break the loop
                        {
                            Console.WriteLine("receiving message");
                            string message = Encoding.ASCII.GetString(receive_task.Result.Buffer); //ReceiveAsync was successful, encode the reply into ASCII and parse
                            Console.WriteLine(message);
                            string[] bulb_data = message.Split(',');

                            string ipAddress = bulb_data[0];
                            string macAddress = bulb_data[1];
                            string typeID = bulb_data[2];

                            var bulb = BulbsFactory.CreateBulb(ipAddress, macAddress, typeID);
                            if (bulb == null) continue; // dont add null bulbs.

                            bulbList.Add(bulb);
                        }
                        else //Cancelled (or timed out), close out socket
                        {

                            Console.WriteLine(bulbList.Count);

                            discovery_client.Close();
                            m_cancelScanSource.Dispose();
                            m_cancelScanSource = null;
                            break;

                        }

                    }
                }

            }
            return bulbList;
        }

    }

    static class BulbsFactory
    {
        public static Bulb CreateBulb(string ipAddress, string macAddress, string typeID)
        {
            Bulb bulb;
            if (macAddress.Contains("DC4F22E1") || macAddress.Contains("5CCF7FE18"))
            {
                bulb = new RGBWWBulb(ipAddress, macAddress, typeID);
                Console.WriteLine("Created a new RGBWW Bulb");
            }
            else if (macAddress.Contains("6001940ED"))
            {
                bulb = new RGBWWStrip(ipAddress, macAddress, typeID);
                Console.WriteLine("Created a new RGBWW Strip");
            }
            else if (macAddress.Contains("DC4F22") || macAddress.Contains("600194"))
            {
                bulb = new RGBWBulb(ipAddress, macAddress, typeID);
                Console.WriteLine("Created a new RGBW Bulb");
            }
            else if (typeID.Contains("ZJ2101"))
            {
                bulb = new RGBWWBulb(ipAddress, macAddress, typeID);
                Console.WriteLine("Created a new RGBWW Small Bulb");
            }
            else
            {
                bulb = null;
                Console.WriteLine("can't determine type");
            }

            return bulb;
        }
    }
}