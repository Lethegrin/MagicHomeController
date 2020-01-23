using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
namespace MagicHomeConsoleApp
{
   public class Program
    {
        public static async Task Main(string[] args)
        {

            Console.WriteLine("call scan");
            Discovery d = new Discovery();
            Animations a = new Animations();
            var scantask = d.Scan(2000, 4);
          

            try
            {
                await scantask;
            }
            catch { }

            foreach (Bulb bulb in d.bulbList)
            {

                Thread.Sleep(500);
                try
                {

                    new Thread(() => {
                        while (true)
                        {
                            var animationTask = Animations.ColorWheel(bulb, 50, 1);
                           // Animations.ColorWheel(bulb,50,1);
                        }
                    }).Start();
                    //}
                }
                catch { }
            }
        }
    }
} 