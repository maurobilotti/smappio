using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Smappio_SEAR
{
    public static class Utils
    {
        public static Stream ToStream(this string text)
        {
            // convert string to stream
            byte[] byteArray = Encoding.UTF8.GetBytes(text);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            return new MemoryStream(byteArray);
        }

        public static byte[] ToByteArray(this string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        public static Stream ToStream(this byte[] byteArray)
        {
            return new MemoryStream(byteArray);
        }

        public static string To8bitBinary(this Int16 num)
        {
            return Convert.ToString(num, 2).PadLeft(8, '0');
        }

        public static string To16bitBinary(this Int16 num)
        {
            return Convert.ToString(num, 2).PadLeft(16, '0');
        }

        public static byte[] GetBytesFromBinaryString(this string binary)
        {
            var list = new List<byte>();

            for (int i = 0; i < binary.Length; i += 8)
            {
                string t = binary.Substring(i, 8);

                list.Add(Convert.ToByte(t, 2));
            }

            return list.ToArray();
        }

        public static float ConvertRange(Int16 originalStart, Int16 originalEnd, float newStart, float newEnd, Int16 value) 
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (float)(newStart + ((value - originalStart) * scale));
        }
    }
}
