using System;
using System.Threading;

namespace MagicHomeController
{
    class RGBWWStrip : Bulb
    {
        public RGBWWStrip(string ipAddress, string macAddress, string bulbID) : base(ipAddress, macAddress, bulbID)
        {
        }
    }
}
