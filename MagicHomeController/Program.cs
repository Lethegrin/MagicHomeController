using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
namespace MagicHomeController
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
                Task t = OpenAllConnections(bulb);
                await t;
            }

            foreach (var bulb in readOnlyList)
            {


                new Thread(() => {
                    while(true)
                    Animations.ColorWheel(bulb, 1000, 1000,1);
                }).Start();

                new Thread(() => {


                    while (true)
                    {

                        bulb.GetState(2);
                        Console.WriteLine($"{bulb.IpAddress} {bulb.tempRed} {bulb.tempGreen} {bulb.tempBlue}");
                    }
                }).Start();
            }
   

        }


        public static Task OpenAllConnections(Bulb bulb)
        {
            bulb.OpenConnections();
            return Task.CompletedTask;
        }

    }
}
