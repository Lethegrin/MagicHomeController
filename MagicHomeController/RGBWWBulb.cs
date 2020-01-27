using System;
using System.Threading;

namespace MagicHomeController
{
    class RGBWWBulb : Bulb
    {
        public RGBWWBulb(string ipAddress, string macAddress, string bulbID) : base(ipAddress, macAddress, bulbID) { }

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