using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicHomeController
{

    /*
     * public class Discovery
     * 
     * This class simply calls out a UDP packet to all bulbs on the network and waits for a response,
     * Bulbs respond with a byte array which is converted to ASCII string'ipaddress,macaddress,typeid'
     * 
     * This class does NOT retrieve the bulb's state in any way (i.e. color, on/off status, etc...)
     * This class needs a lot of work and I don't yet understand the process of how it sorts out...
     * ...multiple responses into single strings.
     * 
     */
    public class Discovery
    {

        private const int DISCOVERY_PORT = 48899; 
        //Port that we listen for bulbs to respond on

        private byte[] MAGIC_UDP_PACKET = Encoding.ASCII.GetBytes("HF-A11ASSISTHREAD"); 
        //Encode magic UDP packet, causes all bulbs to respond


        private List<Bulb> m_discoveredBulbs = new List<Bulb>();
        //list of all bulb objects, bulb class contains state data logic for manipulating bulb color, on/off state etc...

        private CancellationTokenSource m_cancelScanSource;
        //cancelation token to cancel our scan method (Need to look into this, is it like an arduino interrupt?)

            //returns our private list; 
        public List<Bulb> DiscoveredBulbs
        {
            get { return m_discoveredBulbs; }
        }

        public delegate void BulbDiscoveryHandler(Bulb bulb);
        public event BulbDiscoveryHandler DiscoveredBulb;

        /*Scan
         * Sends out a UDP packet over the network and listens for response
         * returns a Task... still need to investigate how this works
         * assuming it returns null until an actual return value is sent,
         * in this case a List of Bulb objects.
         * 
         * @params: 
         * millisecondsTimeout: timeout for each individual bulb if no response is received. (non functional)
         * maxRetries: retries after not hearing from a single bulb (non functional)
         */
        public async Task<List<Bulb>> Scan(int millisecondsTimeout = 2000, int maxRetries = 2)
        {
            //Delete old bulb list
            m_discoveredBulbs.Clear();

            //Create UDP Client for discovery broadcast
            using (UdpClient discovery_client = new UdpClient()) 
            {

                //Send magic packet to get controllers to announce themselves
                IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_PORT);

               

                m_cancelScanSource = new CancellationTokenSource(millisecondsTimeout);
                Console.WriteLine("sending magic packet");
                discovery_client.Send(MAGIC_UDP_PACKET, MAGIC_UDP_PACKET.Length, ip); //Send magic packet to get controllers to announce themselves
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, DISCOVERY_PORT); //Listen for their return packets


                while (true)
                {

                    //Hack in a way to allow a CancellationToken for ReceiveAsync
                    //Based heavily on https://stackoverflow.com/questions/19404199/how-to-to-make-udpclient-receiveasync-cancelable
                    var receive_task = discovery_client.ReceiveAsync();
                    var tcs = new TaskCompletionSource<bool>();

                        //??? confused about the using identifier and wrapping curly brackets
                        using (m_cancelScanSource.Token.Register(s => tcs.TrySetResult(true), null))
                        {
                            //if the cancellation token isn't true continue, else break the loop
                            if (await Task.WhenAny(receive_task, tcs.Task) == receive_task)
                            {

                                //ReceiveAsync was successful, encode the reply into ASCII and parse
                                string message = Encoding.ASCII.GetString(receive_task.Result.Buffer);

                                Console.WriteLine(message);

                                //When encoded to ASCII and converted to a string, data arrives in this pattern:
                                // 'ipaddress,macaddress,typeid'
                                // for example "192.168.1.21,6001940ED006,ZJ2101"

                                // split the data and save to variables
                                string[] bulb_data = message.Split(',');
                                string ipAddress = bulb_data[0];
                                string macAddress = bulb_data[1];
                                string typeID = bulb_data[2];

                                //instantiate a bulb object in "BulbsFactory" Class. Could just as easily be a...
                                //BulbsFactory method.
                                var bulb = BulbsFactory.CreateBulb(ipAddress, macAddress, typeID);

                                // If BulbsFactory can't figure out what the bulb type is, it will set to null
                                // We dont add null bulbs to our list of Bulb objects. So we 'continue',
                                // skipping bulbList.Add(bulb);
                                if (bulb == null) continue;

                                //add our newly created Bulb object to a list so that we may access it later
                                m_discoveredBulbs.Add(bulb);
                                DiscoveredBulb?.Invoke(bulb);
                        }
                            else //Cancelled (or timed out), close out socket
                            {

                                Console.WriteLine(m_discoveredBulbs.Count);
                              

                                //close our port so it doesn't timeout causing errors
                                discovery_client.Close();

                                //??? destroy our cancelationsource
                                m_cancelScanSource.Dispose();
                                //??? set our cancelationsource to null. This may be why scanRetry isn't working
                                m_cancelScanSource = null;

                                //break out of the first  'while (true)' loop as we have no more bulbs to add
                                break;

                            }

                        }
                    }
                
            }
   
                return m_discoveredBulbs;
        }
        public void CancelScan()
        {
            m_cancelScanSource?.Cancel();
        }
    }
    /*
     * BulbsFactory Class
     * @params string ipAddress, string macAddress, string typeID
     * @return Bulb
     * 
     * There are ~4 different types of "Magichome" bulbs with different capabilities
     * I have been able to determine the exact bulb type by looking at a combination of...
     * ...typeID and macAddress, however the real android "Magichome" app MUST have a less 'hacky'...
     * ...way of determining bulb capabilities. Bulbs are separated into three subclasses...
     * ...(RGBWBulb,RGBWWBulb, RGBWWStrip) which I have found to be a convenient way...
     * ...to alter logic as capabilities differ.
     * 
     * Check here line #163 - #187 for possible...
     * less 'hacky' solution https://github.com/NeoSkye/FluxLEDSharp/blob/53a616a0933b48a96f00da06f7aab53ab4dff1ba/libFluxLED/WifiLedBulb.cs
     * 
     * and here https://github.com/AdamoT/MiLightNet/blob/af65b9b3c1760609fc6f822c69b94d61ce16ca84/MilightNet/MiLight.cs for an alternate soltion
     * 
     * Bulb capabilities are listed below.
     * 
     */
    static class BulbsFactory
    {

        //CreateBulb Method could just as easily use a switch statement, makes little difference
        public static Bulb CreateBulb(string ipAddress, string macAddress, string typeID)
        {
            Bulb bulb;

            //RGBWW Large Bulb: Capabilities: RGB, CCT-WarmWhite, CCT-ColdWhite...
            //..., Simultanious CCT-WarmWhite and CCT-ColdWhite (warning, bulb gets hot)
            if (macAddress.Contains("DC4F22E1") || macAddress.Contains("5CCF7FE18"))
            {
                bulb = new RGBWWBulb(ipAddress, macAddress, typeID);
                Console.WriteLine("Created a new RGBWW Large Bulb");
            }

            //RGBWW Strip Controller: Capabilities: RGB, CCT-WarmWhite, CCT-ColdWhite...
            //..., Simultanious CCT-WarmWhite and CCT-ColdWhite (warning, bulb gets hot)
            //..., Simultanious CCT and RGB (WARNING, LEDs draw full power, gets very hot and could burn out power supply)
            else if (macAddress.Contains("6001940ED"))
            {
                bulb = new RGBWWStrip(ipAddress, macAddress, typeID);
                Console.WriteLine("Created a new RGBWW Strip");
            }

            //RGBW Medium Bulb: Capabilities: RGB, CCT-WarmWhite
            else if (macAddress.Contains("DC4F22") || macAddress.Contains("600194"))
            {
                bulb = new RGBWBulb(ipAddress, macAddress, typeID);
                Console.WriteLine("Created a new RGBW Medium Bulb");
            }

            //RGBWW Small Bulb: Capabilities: RGB, CCT-WarmWhite, CCT-ColdWhite...
            //..., Simultanious CCT-WarmWhite and CCT-ColdWhite (warning, bulb gets hot)
            else if (typeID.Contains("ZJ2101"))
            {
                bulb = new RGBWWBulb(ipAddress, macAddress, typeID);
                Console.WriteLine("Created a new RGBWW Small Bulb");
            }
            //Cant determine the type from my 'hacky' macaddress/typeID rullset
            //bulb probably valid, but is a new type.
            //might just default it to basic RGBWBulb subclass rather than setting as null
            else
            {
                bulb = null;
                Console.WriteLine("can't determine type");
            }
            //return the newly created bulb subclass object
            return bulb;
        }
    }
}