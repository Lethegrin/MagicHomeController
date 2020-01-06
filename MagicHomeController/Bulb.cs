using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MagicHomeConsoleApp
{
    abstract public class Bulb
    {

        private Transmit transmit = new Transmit();

        Socket _socket;
        int DefaultPort = 5577;

        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string BulbID { get; set; }

        public Color Color { get; set; }

        public bool IsOn { get; set; }

        public bool IsRGBWW { get; set; }

        public bool IsPersistant { get; set; }

        public byte[] powerOn = { 0x71,
                           0x23,
                           0x0f };
        byte[] powerOff = { 0x71,
                            0x24,
                            0x0f };



        public Bulb(string ipAddress, string macAddress, string bulbID)
        {


            IPEndPoint _endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), DefaultPort);
            _socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //_socket = new Socket(SocketType.Dgram, ProtocolType.Tcp);
            _socket.ReceiveTimeout = 1;
            _socket.SendTimeout = 1;
            _socket.Connect(_endPoint);

            IpAddress = ipAddress;
            MacAddress = macAddress;
            BulbID = bulbID;
            //GetState();


        }

        public void GetState()
        {
            try
            {
                using UdpClient discovery_client = new UdpClient();
                byte[] getStatusMessage = new byte[] { 0x81, 0x8A, 0x8B, 0x96 };
                Console.WriteLine("getting status...");
                byte[] response = transmit.SendMessage(IpAddress, getStatusMessage, false, true);

                if (response.Length != 14)
                {
                    throw new Exception("Controller sent wrong number of bytes while getting status");
                }

                byte persistance = response[0];
                byte bulbType = response[1];
                byte powerState = response[2];
                byte mode = response[3];
                byte presetDelay = response[5];
                byte red = response[6];
                byte green = response[7];
                byte blue = response[8];
                byte warmWhite = response[9];
                byte versionNumber = response[10];
                byte coldWhite = response[11];

                Color color = new Color(red, green, blue, warmWhite, coldWhite);
                bool isOn = (powerState == 0x24 ? true : false);
                bool isRGBWW = (bulbType == 0x35 ? true : false);
                bool isPersistant = (persistance == 0x31 ? true : false);

                Color = new Color(color);
                IsOn = isOn;
                IsRGBWW = isRGBWW;
                IsPersistant = isPersistant;
            } catch (Exception e)
            {

            }
        }

        public virtual void Test()
        {
            byte red = 255;
            byte blue = 0;
            bool forward = true;
            while (true)
            {
                if (red >= 255)
                    forward = false;
                else if (red <= 0)
                    forward = true;

                if (forward)
                {
                    red++;
                    blue--;
                }
                else
                {
                    red--;
                    blue++;
                }

                SetColorAndWhiteLevel(red, 0, blue, red, blue);
                Thread.Sleep(200);

            }
        }

        public void TurnOn()
        {
            IsOn = true;
            UpdateStatePower();

        }

        public void TurnOff()
        {
            IsOn = false;
            UpdateStatePower();
        }

        public void SetColor(Color color)
        {
            Color = new Color(color);
            UpdateStateColor();
        }

        public virtual void SetColorLevel(byte r, byte g, byte b)
        {
            Color.Red = r;
            Color.Green = g;
            Color.Blue = b;
            UpdateStateColor();
        }

        public virtual void SetWarmWhiteLevel(byte w)
        {
            Color.WarmWhite = w;
            UpdateStateWhite();
        }

        public virtual void SetColdWhiteLevel(byte c)
        {
            Color.ColdWhite = c;
            UpdateStateWhite();
        }

        public virtual void SetBothWhiteLevel(byte w, byte c)
        {
            Color.WarmWhite = w;
            Color.ColdWhite = c;
            UpdateStateWhite();
        }

        public virtual void SetColorAndWhiteLevel(byte r, byte g, byte b, byte w, byte c)
        {
            Color.Red = r;
            Color.Green = g;
            Color.Blue = b;
            Color.WarmWhite = w;
            Color.ColdWhite = c;
            UpdateStateColorAndWhite();
        }

        private void UpdateStatePower()
        {
            CreateBasicMessage((IsOn == true ? powerOn : powerOff));
        }

        public void UpdateStateColor()
        {
            CreateColorMessage(0xF0); // sets only color channels to change
        }
        public void UpdateStateWhite()
        {
            CreateColorMessage(0x0F); // sets only white channels to change
        }
        public void UpdateStateColorAndWhite()
        {
            CreateColorMessage(0xFF);
        }

        public virtual void CreateColorMessage(byte mask)
        {
            byte[] sendMessageByte;

            sendMessageByte = new byte[]
            {
                0x31,
                Color.Red, // Red byte
                Color.Green, //3 Green byte
                Color.Blue, //4 Blue byte
                Color.WarmWhite, //5 WarmWhite byte
                Color.ColdWhite, //6 ColdWhite byte
                mask, //7
                0x0F  //8 terminator (I'll be back)
            };

            CreateBasicMessage(sendMessageByte);
        }

        public void CreateBasicMessage(byte[] message)
        {

            try
            {
                SendMessage(IpAddress, message, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e + " exception found for ip address: " + IpAddress);
            }

        }

        public byte CalculateChecksum(byte[] bytes)
        {
            byte checksum = 0;

            foreach (var b in bytes)
            {
                unchecked
                {
                    checksum += b;
                }
            }

            return checksum;
        }


        public void SendMessage(string ipAddress, byte[] bytes, bool sendChecksum)
        {


            if (sendChecksum)
            {
                var checksum = CalculateChecksum(bytes);
                Array.Resize(ref bytes, bytes.Length + 1);
                bytes[bytes.Length - 1] = checksum;
            }

            _socket.Send(bytes);
        }

    }
}
