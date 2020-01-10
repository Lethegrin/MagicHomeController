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
            var scantask = d.Scan(2000, 3);

            try
            {
                await scantask;
            }
            catch { }




          
                foreach (Bulb bulb in d.bulbList)
                {

                    try
                    {

           //         if (bulb.TypeID.Contains("ZJ2101"))
            //        {
                        new Thread(() =>
                                {
                                    while (true)
                                    {




                                        Lantern(bulb);
                                }


                                }
                                          ).Start();


                    }
                    //}
                    catch { }




                


            }
        }
        // Generate a random number between two numbers  
        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
        public static byte RandomNumber(byte min, byte max)
        {
            Random random = new Random();
            return (byte)random.Next(min, max);
        }
        public static double RandomNumber(double min, double max)
        {
            Random random = new Random();
            return random.NextDouble() * (max - min) + min;
        }

        public static void Lightning(Bulb bulb)
        {
       
                bulb.SetColorAndWhiteLevel(0, 0, 0, 0, 0);
            

                    Thread.Sleep(RandomNumber(60000, 120000));
                    bulb.SetColdWhiteLevel(RandomNumber((byte)100, (byte)255));
                    Thread.Sleep(RandomNumber(10, 100));
                    bulb.SetColorAndWhiteLevel(0, 0, 0, 0, 0);
            Thread.Sleep(25);
            int randomChance = RandomNumber(1, 3);
                    if (randomChance == 3)
                        bulb.SetColdWhiteLevel(RandomNumber((byte)100, (byte)255));

                


        }

        public static void Firefly(Bulb bulb)
        {

             bulb.SetColorLevel(0, 0, 0);
            Color firelyColor = new Color(140, 255, 0, 0, 0);
            int DelayBetweenPulses = RandomNumber(30000, 120000), openDelaySpeed = RandomNumber(20, 50), closeDelaySpeed = RandomNumber(5,10);


                bulb.SetColorLevel(0, 0, 0);

                    Thread.Sleep(DelayBetweenPulses);

                    Flash(bulb, firelyColor, openDelaySpeed, 0, 1.0);
                 Flash(bulb, firelyColor, closeDelaySpeed, 1.0, 0);
                Thread.Sleep(100);
                bulb.SetColorLevel(0, 0, 0);
            }

        public static void Lantern(Bulb bulb)
        {

            bulb.SetColorAndWhiteLevel(0, 0, 0,0,0);
            Color randomBlue = new Color(RandomNumber((byte)0, (byte)20), RandomNumber((byte)0, (byte)255), RandomNumber((byte)50, (byte)255), 0, 0);
            Color randomPink = new Color(255, 0, RandomNumber((byte)5, (byte)220), 0, 0);
            int DelayBetweenPulses = RandomNumber(1000, 30000), openDelaySpeed = RandomNumber(10, 20), closeDelaySpeed = RandomNumber(1, 10);


            Thread.Sleep(DelayBetweenPulses);

            int randomChance = RandomNumber((int)0, (int)2);
            Console.WriteLine(randomChance);
            if (randomChance == 1)
            {
                Console.WriteLine(randomChance);
                Flash(bulb, randomBlue, openDelaySpeed, 0, 1.0);
                Flash(bulb, randomBlue, closeDelaySpeed, 1.0, 0);
            }
            else
            {
                Flash(bulb, randomPink, openDelaySpeed, 0, 1.0);
                Flash(bulb, randomPink, closeDelaySpeed, 1.0, 0);
            }

            bulb.SetColorAndWhiteLevel(0, 0, 0,0,0);
        }




        public static void Flash(Bulb bulb, Color color, int delay, double rawStartLuminocity, double rawEndLuminocity)
        {
            double startLuminocity = LuminocityClamp(rawStartLuminocity);
            double endLuminocity = LuminocityClamp(rawEndLuminocity);

//            Console.WriteLine($"raw luminocity: {rawEndLuminocity} --- fixed luminocity: {endLuminocity}");
            double currentLuminocity = startLuminocity;

            while (true)
            {

                Thread.Sleep(delay);
                bulb.SetColorAndWhiteLevel(SetLuminocity(color, LuminocityClamp(currentLuminocity)));


                if (rawStartLuminocity < rawEndLuminocity && currentLuminocity < endLuminocity)
                
                    currentLuminocity += .01;

                else if (rawStartLuminocity > rawEndLuminocity && currentLuminocity > endLuminocity)
                
                    currentLuminocity -= .01;
                
                else
                    break;

               // Console.WriteLine($"raw luminocity: {currentLuminocity}");





            }
        }

        public static Color SetLuminocity(Color color, double luminocity)
        {
            Color newColor = new Color( (byte)(color.Red * luminocity),
                                        (byte)(color.Green * luminocity),
                                        (byte)(color.Blue * luminocity),
                                        (byte)(color.WarmWhite * luminocity),
                                        (byte)(color.ColdWhite * luminocity));

            return newColor;
        }

        public static double LuminocityClamp(double rawNumber)
        {
            double number = rawNumber;
            return Math.Clamp(number, 0.0, 1.0);

        }
    }
}

