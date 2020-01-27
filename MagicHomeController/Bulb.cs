using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MagicHomeController
{
    abstract public class Bulb
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

       public byte tempRed = 0;
        public byte tempGreen = 0;
        public byte tempBlue = 0;


        private Connection sendConnection;
        private Connection receiveConnection;

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

        public Colors Colors
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
        internal Connection SendConnection { get => sendConnection; set => sendConnection = value; }
        internal Connection ReceiveConnection { get => receiveConnection; set => receiveConnection = value; }

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
 

            IpAddress = ipAddress;
            MacAddress = macAddress;
            TypeID = bulbID;
            Colors = new Colors();

        }

        public bool GetState(int retries = 5)
        {

            byte[] response = receiveConnection.QueryState(retries);
       

                if (response == null) 
                {
                Console.WriteLine("Returned Null");
                return false;
                } else if (response.Length != 14)
            {
                Console.WriteLine("Incorrect response length");
                return false;
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
            tempRed = response[6];
            tempGreen = response[7];
            tempBlue = response[8];

            Colors.Red = red;
            Colors.Green = green;
            Colors.Blue = blue;
            Colors.WarmWhite = warmWhite;
            Colors.ColdWhite = coldWhite;

        //    Console.WriteLine($"The current state of IpAddress: {IpAddress} -- red: {red} -- green: {green} -- blue: {blue}");
               
                bool isOn = (powerState == 0x24 ? true : false);
                bool isRGBWW = (bulbType == 0x35 ? true : false);
                bool isPersistant = (persistance == 0x31 ? true : false);

                IsOn = isOn;
                IsRGBWW = isRGBWW;
                IsPersistant = isPersistant;

            return true;
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
            Colors.Red = r;
            Colors.Green = g;
            Colors.Blue = b;
            UpdateStateColor();
        }

        public virtual void SetWarmWhiteLevel(byte w, bool persistance = false)
        {
            Colors.WarmWhite = w;
            UpdateStateWhite();
        }

        public virtual void SetColdWhiteLevel(byte c, bool persistance = false)
        {
            Colors.ColdWhite = c;
            UpdateStateWhite();
        }

        public virtual void SetBothWhiteLevel(byte w, byte c, bool persistance = false)
        {
            Colors.WarmWhite = w;
            Colors.ColdWhite = c;
            UpdateStateWhite();
        }

        public virtual void SetColorAndWhiteLevel(byte r, byte g, byte b, byte w, byte c, bool persistance = false)
        {
            Colors.Red = r;
            Colors.Green = g;
            Colors.Blue = b;
            Colors.WarmWhite = w;
            Colors.ColdWhite = c;
            UpdateStateColorAndWhite();
        }

        public virtual void SetColorAndWhiteLevel(Colors color, bool persistance = false)
        {
            Colors.Red = color.Red;
            Colors.Green = color.Green;
            Colors.Blue = color.Blue;
            Colors.WarmWhite = color.WarmWhite;
            Colors.ColdWhite = color.ColdWhite;
            UpdateStateColorAndWhite();
        }

        public void UpdateStatePower()
        {
            sendConnection.SendMessage((IsOn == true ? powerOn : powerOff),false);
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
    Colors.Red, // Red byte
    Colors.Green, //3 Green byte
    Colors.Blue, //4 Blue byte
    Colors.WarmWhite, //5 WarmWhite byte
    Colors.ColdWhite, //6 ColdWhite byte
    mask, //7
    0x0F //8 terminator (I'll be back)
   };

            sendConnection.SendMessage(sendMessageByte, true);
        }

        public virtual void OpenConnections()
        {
            receiveConnection = new Connection(this);
            sendConnection = new Connection(this);
        }



    }
}