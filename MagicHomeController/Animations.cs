using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicHomeConsoleApp
{
    public class Animations
    {


        public static void Lightning(Bulb bulb)
        {

            bulb.SetColorAndWhiteLevel(0, 0, 0, 0, 0);


            Thread.Sleep(RandomNumber(5000, 50000));
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
            Colors firelyColor = new Colors(140, 255, 0, 0, 0);
            int DelayBetweenPulses = RandomNumber(5000, 10000), openDelaySpeed = RandomNumber(20, 50), closeDelaySpeed = RandomNumber(5, 10);




            Thread.Sleep(DelayBetweenPulses);

            Flash(bulb, firelyColor, openDelaySpeed, 0, 1.0);
            Flash(bulb, firelyColor, closeDelaySpeed, 1.0, 0);
            Thread.Sleep(100);
            bulb.SetColorLevel(0, 0, 0);
        }

        public static void Lantern(Bulb bulb)
        {

            bulb.SetColorAndWhiteLevel(0, 0, 0, 0, 0);
            Colors randomBlue = new Colors(RandomNumber((byte)0, (byte)20), RandomNumber((byte)0, (byte)255), RandomNumber((byte)50, (byte)255), 0, 0);
            Colors randomPink = new Colors(255, 0, RandomNumber((byte)5, (byte)220), 0, 0);
            int DelayBetweenPulses = RandomNumber(1000, 5000), openDelaySpeed = RandomNumber(10, 20), closeDelaySpeed = RandomNumber(1, 10);


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

        }

        public static void FlyingLantern(Bulb bulb, double luminocity)
        {
            bulb.SetColorLevel(0, 0, 0);
            int openDelaySpeed = RandomNumber(100, 200),
             delayBetweenPulses = RandomNumber(1, 60000),
             twinkles = RandomNumber(5, 20);
            Console.WriteLine(delayBetweenPulses);

            Thread.Sleep(delayBetweenPulses);

            Colors randomBlue = new Colors(RandomNumber((byte)0, (byte)20), RandomNumber((byte)0, (byte)255), RandomNumber((byte)50, (byte)255), 0, 0);
            Colors randomPink = new Colors(255, 0, RandomNumber((byte)5, (byte)220), 0, 0);
            Colors transitionColor;



            int randomChance = RandomNumber((int)0, (int)2);

            if (randomChance == 1)
                transitionColor = randomPink;
            else
                transitionColor = randomBlue;

            ColorTransition(bulb, bulb.Color, transitionColor, openDelaySpeed, luminocity);

            for (int i = 0; i < twinkles; i++)
            {
                int twinkleSpeed = RandomNumber(10, 25);
                double dimLuminocity = RandomNumber(.40, .65);
                int brightDuration = RandomNumber(1000, 5000);


                Flash(bulb, transitionColor, twinkleSpeed, luminocity, luminocity * dimLuminocity);
                Flash(bulb, transitionColor, twinkleSpeed, luminocity * dimLuminocity, luminocity);
                Thread.Sleep(brightDuration);
            }

        }

        public static Colors ColorTransition(Bulb bulb, Colors openColor, Colors closeColor, int delay, double luminocity)
        {
            Colors currentColor = new Colors();
            double currentRatio = 0;

            while (true)
            {

                Thread.Sleep(delay);


                currentColor.Red = (byte)Math.Abs((currentRatio * closeColor.Red) + ((1 - currentRatio) * openColor.Red));
                currentColor.Green = (byte)Math.Abs((currentRatio * closeColor.Green) + ((1 - currentRatio) * openColor.Green));
                currentColor.Blue = (byte)Math.Abs((currentRatio * closeColor.Blue) + ((1 - currentRatio) * openColor.Blue));
                currentColor.WarmWhite = (byte)Math.Abs((currentRatio * closeColor.WarmWhite) + ((1 - currentRatio) * openColor.WarmWhite));
                currentColor.ColdWhite = (byte)Math.Abs((currentRatio * closeColor.ColdWhite) + ((1 - currentRatio) * openColor.ColdWhite));


                if (currentRatio < 1)
                {
                    bulb.SetColorAndWhiteLevel(SetLuminocity(currentColor, LuminocityClamp(luminocity)));
                    currentRatio = Math.Clamp(currentRatio + .001, 0.0, 1.0);
                }
                else
                {
                    Thread.Sleep(20);
                    bulb.SetColorAndWhiteLevel(SetLuminocity(currentColor, LuminocityClamp(luminocity)));
                    break;
                }

            }

            return bulb.Color;
        }

        public static Colors ColorWheel(Bulb bulb, int delay, int luminocity)
        {

            Colors red = new Colors(255, 0, 0, 0, 0);
            Colors green = new Colors(0, 255, 0, 0, 0);
            Colors blue = new Colors(0, 0, 255, 0, 0);

            Animations.ColorTransition(bulb, red, blue, delay, luminocity);
            Animations.ColorTransition(bulb, blue, green, delay, luminocity);
            Animations.ColorTransition(bulb, green, red, delay, luminocity);

            return bulb.Color;

        }


        public static Colors Flash(Bulb bulb, Colors color, int delay, double startLuminocity, double endLuminocity)
        {
            startLuminocity = Math.Clamp(startLuminocity, 0.0, 1.0);
            endLuminocity = Math.Clamp(endLuminocity, 0.0, 1.0);

            double currentLuminocity = startLuminocity;

            while (true)
            {

                Thread.Sleep(delay);

                if (startLuminocity < endLuminocity && currentLuminocity < endLuminocity)

                    currentLuminocity += .01;

                else if (startLuminocity > endLuminocity && currentLuminocity > endLuminocity)

                    currentLuminocity -= .01;

                else
                {
                    Thread.Sleep(20);
                    bulb.SetColorAndWhiteLevel(SetLuminocity(color, LuminocityClamp(endLuminocity)));
                    return bulb.Color;
                }

                bulb.SetColorAndWhiteLevel(SetLuminocity(color, LuminocityClamp(currentLuminocity)));

            }

        }

        public static Colors SetLuminocity(Colors color, double luminocity)
        {
            Colors newColor = new Colors((byte)(color.Red * luminocity),
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
    }
}