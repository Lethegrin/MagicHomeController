using System;
using System.Threading;

namespace MagicHomeConsoleApp
{
    class RGBWWBulb : Bulb
    {
        public RGBWWBulb(string ipAddress, string macAddress, string bulbID) : base(ipAddress, macAddress, bulbID)
        {
        }

        public override void SetColorAndWhiteLevel(byte r, byte g, byte b, byte w, byte c)
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
