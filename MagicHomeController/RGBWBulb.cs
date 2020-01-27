using System;
using System.Threading;

namespace MagicHomeController
{
    class RGBWBulb : Bulb
    {

        public RGBWBulb(string ipAddress, string macAddress, string bulbID) : base(ipAddress, macAddress, bulbID)
        {

        }
        public override void CreateColorMessage(byte mask)
        {
            byte[] sendMessageByte;

            // 7 byte message for RGBW bulbs
            sendMessageByte = new byte[] {
    0x31,
    Colors.Red, // Red byte
    Colors.Green, //3 Green byte
    Colors.Blue, //4 Blue byte
    Colors.WarmWhite, //5 WarmWhite byte
    mask, //6 Mask
    0x0F //7 terminator (I'll be back)
   };

            SendConnection.SendMessage(sendMessageByte, true);
        }
        public override void SetColorLevel(byte r = 0, byte g = 0, byte b = 0, bool persistance = false)
        {
            Colors.Red = r;
            Colors.Green = g;
            Colors.Blue = b;
            UpdateStateColor();
        }
        public override void SetWarmWhiteLevel(byte w, bool persistance = false)
        {
            Colors.WarmWhite = w;
            UpdateStateWhite();
        }
        public override void SetColdWhiteLevel(byte c, bool persistance = false)
        {
            Colors.ColdWhite = c;
            Colors.WarmWhite = c;
            UpdateStateWhite();
        }
        public override void SetBothWhiteLevel(byte w, byte c, bool persistance = false)
        {
            int intWarmWhiteValue = (int)(w + c) / 2;
            byte byteWarmWhiteValue = (byte)intWarmWhiteValue;
            Colors.WarmWhite = byteWarmWhiteValue;
            Colors.ColdWhite = c;
            UpdateStateWhite();
        }
        public override void SetColorAndWhiteLevel(byte r = 0, byte g = 0, byte b = 0, byte w = 0, byte c = 0, bool persistance = false)
        {
            Colors.Red = r;
            Colors.Green = g;
            Colors.Blue = b;
            Colors.WarmWhite = w;
            Colors.ColdWhite = c;

            if (r > 0 || g > 0 || b > 0)
                UpdateStateColor();
            else
                UpdateStateWhite();
        }
        public override void SetColorAndWhiteLevel(Colors color, bool persistance = false)
        {
            Colors.Red = color.Red;
            Colors.Green = color.Green;
            Colors.Blue = color.Blue;
            Colors.WarmWhite = color.WarmWhite;
            Colors.ColdWhite = color.ColdWhite;
            if (color.Red > 0 || color.Green > 0 || color.Blue > 0)

                UpdateStateColor();
            else
                UpdateStateWhite();
        }

    }
}