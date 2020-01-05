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
        private const int DISCOVERY_PORT = 48899;
        public List<Bulb> bulbList = new List<Bulb>();
        private CancellationTokenSource m_cancelScanSource;

        public async Task Scan(int millisecondsTimeout)
        {


            //Create UDP Client for discovery broadcast
            using (UdpClient discovery_client = new UdpClient())
            {
                //Send magic packet to get controllers to announce themselves
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT);
                byte[] bytes = Encoding.ASCII.GetBytes("HF-A11ASSISTHREAD");
                discovery_client.Send(bytes, bytes.Length, ip);

                //Listen for their return packets
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, DISCOVERY_PORT);
                m_cancelScanSource = new CancellationTokenSource(millisecondsTimeout);
                int numBulbs = 0;
                while (true)
                {
                    //Hack in a way to allow a CancellationToken for ReceiveAsync
                    //Based heavily on https://stackoverflow.com/questions/19404199/how-to-to-make-udpclient-receiveasync-cancelable
                    var receive_task = discovery_client.ReceiveAsync();
                    var tcs = new TaskCompletionSource<bool>();
                    using (m_cancelScanSource.Token.Register(s => tcs.TrySetResult(true), null))
                    {
                        if (await Task.WhenAny(receive_task, tcs.Task) == receive_task)
                        {
                            //ReceiveAsync was successful, parse the reply
                            string message = Encoding.ASCII.GetString(receive_task.Result.Buffer);
                            Console.WriteLine(message);
                            string[] bulb_data = message.Split(',');

                            string ipAddress = bulb_data[0];
                            string macAddress = bulb_data[1];
                            string typeID = bulb_data[2];
                            Bulb bulb;

                            if (macAddress.Contains("DC4F22E1") || macAddress.Contains("5CCF7FE18"))
                            {
                                bulb = new RGBWWBulb(ipAddress, macAddress, typeID);
                            }
                            else if (macAddress.Contains("6001940ED"))
                            {
                                bulb = new RGBWWStrip(ipAddress, macAddress, typeID);
                            }
                            else if (macAddress.Contains("DC4F22") || macAddress.Contains("600194"))
                            {
                                bulb = new RGBWBulb(ipAddress, macAddress, typeID);
                                Console.WriteLine("Created a new RGBW Bulb");
                            }
                            else
                            {
                                bulb = null;
                                Console.WriteLine("can't determine type");
                            }


                            bulbList.Add(bulb);

                            numBulbs++;
                        }
                        else
                        {
                            Console.WriteLine(numBulbs);
                            //Cancelled (or timed out), close out socket
                            discovery_client.Close();
                            m_cancelScanSource.Dispose();
                            m_cancelScanSource = null;
                            break;
                        }
                    }

                }

            }
        }





    }
}