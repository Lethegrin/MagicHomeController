using System;
using System.Threading;

namespace MagicHomeConsoleApp
{
    class RGBWWBulb : Bulb
    {
        public RGBWWBulb(string ipAddress, string macAddress, string bulbID) : base(ipAddress, macAddress, bulbID)
        {
        }

        public override void SetColorAndWhiteLevel(byte r = 0, byte g = 0, byte b = 0, byte w = 0, byte c = 0)
        {
            Color.Red = r;
            Color.Green = g;
            Color.Blue = b;
            Color.WarmWhite = w;
            Color.ColdWhite = c;
            UpdateStateColor();
        }

    }
}
