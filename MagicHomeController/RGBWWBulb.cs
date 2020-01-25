using System;
using System.Threading;

namespace MagicHomeConsoleApp
{
    class RGBWWBulb : Bulb
    {
        public RGBWWBulb(string ipAddress, string macAddress, string bulbID) : base(ipAddress, macAddress, bulbID) { }

        public override void SetColorAndWhiteLevel(byte r = 0, byte g = 0, byte b = 0, byte w = 0, byte c = 0, bool persistance = false)
        {
            Color.Red = r;
            Color.Green = g;
            Color.Blue = b;
            Color.WarmWhite = w;
            Color.ColdWhite = c;

            if (r > 0 || g > 0 || b > 0)
                UpdateStateColor();
            else
                UpdateStateWhite();
        }

        public override void SetColorAndWhiteLevel(Colors color, bool persistance = false)
        {
            Color.Red = color.Red;
            Color.Green = color.Green;
            Color.Blue = color.Blue;
            Color.WarmWhite = color.WarmWhite;
            Color.ColdWhite = color.ColdWhite;

            if (color.Red > 0 || color.Green > 0 || color.Blue > 0)
                UpdateStateColor();
            else
                UpdateStateWhite();
        }

    }
}