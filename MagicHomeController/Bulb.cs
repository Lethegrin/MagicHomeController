using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MagicHomeConsoleApp
{
    abstract public class Bulb : IDisposable
    {

        private static byte COLOR_ONLY_WRITEMASK = 0xF0;
        private static byte WHITE_ONLY_WRITEMASK = 0x0F;
        private static byte COLOR_AND_WHITE_WRITEMASK = 0x00;

        private static readonly byte[] NEW_QUERY_MSG = { 0x81, 0x8a, 0x8b, 0x96 };
        private static readonly byte[] OLD_QUERY_MSG = { 0xef, 0x01, 0x77 };

        private static readonly byte[] OLD_ON_MSG = { 0xcc, 0x23, 0x33 };
        private static readonly byte[] NEW_ON_MSG = { 0x71, 0x23, 0x0f };

        private static readonly byte[] OLD_OFF_MSG = { 0xcc, 0x24, 0x33 };
        private static readonly byte[] NEW_OFF_MSG = { 0x71, 0x24, 0x0f };

        private static readonly byte[] GET_CLOCK_MSG = { 0x11, 0x1a, 0x1b, 0x0f };
        private static readonly byte[] GET_TIMERS_MSG = { 0x22, 0x2a, 0x2b, 0x0f };


        private const int WIFI_PORT = 5577;
        Socket m_socket;

        private IPAddress m_ipAddress;

        private int m_timeout = 5000;
        private int m_queryLen;

        public string IpAddress
        {
            get;
            set;
        }
        public string MacAddress
        {
            get;
            set;
        }
        public string TypeID
        {
            get;
            set;
        }

        public Colors Color
        {
            get;
            set;
        }

        public bool IsOn
        {
            get;
            set;
        }

        public bool IsRGBWW
        {
            get;
            set;
        }

        public bool IsPersistant
        {
            get;
            set;
        }

        private byte GetPersistance()
        {
            byte persistanceByte = (IsPersistant == true ? (byte)0x31 : (byte)0x41);
            return persistanceByte;
        }

        private byte[] powerOn = {
   0x71,
   0x23,
   0x0f
  };
       private byte[] powerOff = {
   0x71,
   0x24,
   0x0f
  };


        public Bulb(string ipAddress, string macAddress, string bulbID) 
        {
       
            m_queryLen = 14;
            m_ipAddress = IPAddress.Parse(ipAddress);
            m_socket = new Socket(m_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            IpAddress = ipAddress;
            MacAddress = macAddress;
            TypeID = bulbID;
            Color = new Colors();
            if (!m_socket.Connected)
                Connect();
        }

        public void Connect()
        {
            if (m_socket.Connected)
            {
                m_socket.Close();
                m_socket = new Socket(m_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }

            m_socket.Connect(m_ipAddress, WIFI_PORT);
        }

        public void GetState(int retries = 2)
        {
            if (!m_socket.Connected)
                Connect();
            byte[] response = QueryState(retries);
         
        
                Console.WriteLine("getting status...");

                if (response == null) 
                {
                return;
                } else if (response.Length != 14)
            {
                return;
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


            Color.Red = red;
            Color.Green = green;
            Color.Blue = blue;
            Color.WarmWhite = warmWhite;
            Color.ColdWhite = red;

            Console.WriteLine($"The current state of IpAddress: {IpAddress} -- red: {red} -- green: {green} -- blue: {blue}");
               
                bool isOn = (powerState == 0x24 ? true : false);
                bool isRGBWW = (bulbType == 0x35 ? true : false);
                bool isPersistant = (persistance == 0x31 ? true : false);

                IsOn = isOn;
                IsRGBWW = isRGBWW;
                IsPersistant = isPersistant;
            

        }

        private byte[] QueryState(int retries)
        {

            try
            {
                if (!m_socket.Connected)
                    Connect();
                SendMessage(NEW_QUERY_MSG,false);
                return ReadMsg(m_queryLen);
            }
            catch (SocketException)
            {
                if (retries < 1)
                {
                    IsOn = false;
                    return null;
                }
                return QueryState(retries - 1);
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

        public virtual void SetColorLevel(byte r = 0, byte g = 0, byte b = 0, bool persistance = false)
        {
            Color.Red = r;
            Color.Green = g;
            Color.Blue = b;
            UpdateStateColor();
        }

        public virtual void SetWarmWhiteLevel(byte w, bool persistance = false)
        {
            Color.WarmWhite = w;
            UpdateStateWhite();
        }

        public virtual void SetColdWhiteLevel(byte c, bool persistance = false)
        {
            Color.ColdWhite = c;
            UpdateStateWhite();
        }

        public virtual void SetBothWhiteLevel(byte w, byte c, bool persistance = false)
        {
            Color.WarmWhite = w;
            Color.ColdWhite = c;
            UpdateStateWhite();
        }

        public virtual void SetColorAndWhiteLevel(byte r, byte g, byte b, byte w, byte c, bool persistance = false)
        {
            Color.Red = r;
            Color.Green = g;
            Color.Blue = b;
            Color.WarmWhite = w;
            Color.ColdWhite = c;
            UpdateStateColorAndWhite();
        }

        public virtual void SetColorAndWhiteLevel(Colors color, bool persistance = false)
        {
            Color.Red = color.Red;
            Color.Green = color.Green;
            Color.Blue = color.Blue;
            Color.WarmWhite = color.WarmWhite;
            Color.ColdWhite = color.ColdWhite;
            UpdateStateColorAndWhite();
        }

        private void UpdateStatePower()
        {
            SendMessage((IsOn == true ? powerOn : powerOff),false);
        }

        public void UpdateStateColor()
        {
            CreateColorMessage(COLOR_ONLY_WRITEMASK); // sets only color channels to change
        }
        public void UpdateStateWhite()
        {
            CreateColorMessage(WHITE_ONLY_WRITEMASK); // sets only white channels to change
        }
        public void UpdateStateColorAndWhite()
        {
            CreateColorMessage(COLOR_AND_WHITE_WRITEMASK);
        }

        public virtual void CreateColorMessage(byte mask)
        {
            byte[] sendMessageByte;

            sendMessageByte = new byte[] {
    GetPersistance(), //peristance byte set by bulb.IsPerstant Perameter
    Math.Clamp(Color.Red, (byte) 0, (byte) 255), // Red byte
    Math.Clamp(Color.Green, (byte) 0, (byte) 255), //3 Green byte
    Color.Blue, //4 Blue byte
    Color.WarmWhite, //5 WarmWhite byte
    Color.ColdWhite, //6 ColdWhite byte
    mask, //7
    0x0F //8 terminator (I'll be back)
   };

            SendMessage(sendMessageByte, true);
        }


        private byte[] ReadMsg(int expected)
        {
            byte[] buffer = new byte[expected];
            m_socket.ReceiveTimeout = m_timeout;
            int bytes_read = m_socket.Receive(buffer);
            Array.Resize(ref buffer, bytes_read);
            return buffer;
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

        public void SendMessage(byte[] bytes, bool sendChecksum)
        {


            if (sendChecksum)
            {
                var checksum = CalculateChecksum(bytes);
                Array.Resize(ref bytes, bytes.Length + 1);
                bytes[bytes.Length - 1] = checksum;
            }

            m_socket.Send(bytes);


        }

        public void Dispose()
        {
            if (m_socket.Connected)
            {
                m_socket.Close();
            }
            m_socket.Dispose();
        }
    }
}