using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
namespace MagicHomeConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {

            Console.WriteLine("call scan");
            Discovery d = new Discovery();
            var scantask = d.Scan(2000, 4);

            try
            {
                await scantask;
            }
            catch { }

            foreach (Bulb bulb in d.bulbList)
            {
                try
                {

                    //if (bulb.IpAddress.Contains("21")) // test only one bulb in the house so I don't annoy my girlfriend
                    //{
                    new Thread(() => {
                        while (true)
                        {
                            Animations.ColorWheel(bulb, 50, 1);
                        }
                    }).Start();
                //}
                }
                catch { }
            }
        }
    }
}