using System;
using System.Collections.Generic;
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
            var scantask = d.Scan(2000, 4);

            try
            {
                await scantask;
            }
            catch { }

            IReadOnlyList<Bulb> readOnlyList = await scantask;
            foreach (var bulb in readOnlyList)
            {

                try
                {

                      new Thread(() => {
                          while (true)
                          {
                              bulb.GetState();
                              Thread.Sleep(1000);
                      }
                      }).Start();
                     
                    new Thread(() => {
                      while (true)
                      {
                    Animations.ColorWheel(bulb,500,1);

                      }

                }).Start();
            
                }
                catch { }
            }
        }
    }
} 