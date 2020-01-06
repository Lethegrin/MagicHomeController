using System;

namespace MagicHomeConsoleApp
{

    public class Color
    {
        byte red = 0;
        byte green = 0;
        byte blue = 0;
        byte warmWhite = 0;
        byte coldWhite = 0;



        const byte MAX_BYTE_VALUE = 0xFF;
        const byte MIN_BYTE_VALUE = 0x00;

        public Color(byte red = 0, byte green = 0, byte blue = 0, byte warmWhite = 0, byte coldWhite = 0)
        {
            Red = red;
            Green = green;
            Blue = blue;
            WarmWhite = warmWhite;
            ColdWhite = coldWhite;

        }

        public Color(Color color)
        {
            Red = color.Red;
            Green = color.Green;
            Blue = color.Blue;
            WarmWhite = color.WarmWhite;
            ColdWhite = color.ColdWhite;
        }


        public byte Red
        {
            get { return red; }
            set
            {
                red = Math.Clamp(value, MIN_BYTE_VALUE, MAX_BYTE_VALUE);
            }
        }
        public byte Green
        {
            get { return green; }
            set
            {
                green = Math.Clamp(value, MIN_BYTE_VALUE, MAX_BYTE_VALUE);
            }
        }
        public byte Blue
        {
            get { return blue; }
            set
            {
                blue = Math.Clamp(value, MIN_BYTE_VALUE, MAX_BYTE_VALUE);
            }
        }
        public byte WarmWhite
        {
            get { return warmWhite; }
            set
            {
                warmWhite = Math.Clamp(value, MIN_BYTE_VALUE, MAX_BYTE_VALUE);
            }
        }
        public byte ColdWhite
        {
            get { return coldWhite; }
            set
            {
                coldWhite = Math.Clamp(value, MIN_BYTE_VALUE, MAX_BYTE_VALUE);
            }
        }
    }



}