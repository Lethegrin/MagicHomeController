using System;
using System.Threading;
using System.Threading.Tasks;

namespace MagicHomeConsoleApp
{
    class Program
    {
        public static Color GREEN = new Color(0x00, 0xFF, 0x00, 200, 200);
        public static Color RED = new Color(0xFF, 0x00, 0x00, 200, 200);
        public static Color BLUE = new Color(0x00, 0x00, 0xFF, 200, 200);

        static async Task Main(string[] args)
        {

            Console.WriteLine("call scan");
            Discovery d = new Discovery();
            var scantask = d.Scan(2000);
            await scantask;

            foreach (Bulb bulb in d.bulbList)
            {

                new Thread(() =>
                {
                    bulb.Test();
                }
                ).Start();

            }
        }
    }
}

