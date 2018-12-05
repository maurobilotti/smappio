using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

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
        public static byte[] ToByteArray(this Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }

        public static unsafe byte[] GetBytes(this float value)
        {
            var bytes = new byte[4];
            fixed (byte* b = bytes)
                *((int*)b) = *(int*)&value;

            return bytes;
        }

        public static unsafe byte[] GetBytes(this int value)
        {
            var bytes = new byte[4];
            fixed (byte* b = bytes)
                *((int*)b) = *(int*)&value;

            return bytes;
        }

        public static string ToHexString(this float f)
        {
            var bytes = BitConverter.GetBytes(f);
            var i = BitConverter.ToInt32(bytes, 0);
            return "0x" + i.ToString("X8");
        }
       
    }
}
