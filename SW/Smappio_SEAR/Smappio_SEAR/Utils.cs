using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

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

        public static string ByteListToString(this List<byte> bytes)
        {
            string text = "";
            foreach (byte item in bytes)
            {
                text += Convert.ToInt32(item).ToString() + " ";
            }
            return text;
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

        public static float[] Convert16BitToFloat(byte[] input)
        {
            int inputSamples = input.Length / 2; // 16 bit input, so 2 bytes per sample
            float[] output = new float[inputSamples];
            int outputIndex = 0;
            for (int n = 0; n < inputSamples; n++)
            {
                short sample = BitConverter.ToInt16(input, n * 2);
                output[outputIndex++] = sample / 32768f;
            }
            return output;
        }

        private static byte[] MonoToStereo(byte[] input)
        {
            byte[] output = new byte[input.Length * 2];
            int outputIndex = 0;
            for (int n = 0; n < input.Length; n += 2)
            {
                // copy in the first 16 bit sample
                output[outputIndex++] = input[n];
                output[outputIndex++] = input[n + 1];
                // now copy it in again
                output[outputIndex++] = input[n];
                output[outputIndex++] = input[n + 1];
            }
            return output;
        }

        //public static void CreateWav()
        //{
        //    uint numsamples = 44100;
        //    ushort numchannels = 1;
        //    ushort samplelength = 1; // in bytes
        //    uint samplerate = 22050;

        //    FileStream f = new FileStream("a.wav", FileMode.Create);
        //    BinaryWriter wr = new BinaryWriter(f);

        //    wr.Write("RIFF");
        //    wr.Write(36 + numsamples * numchannels * samplelength);
        //    wr.Write("WAVEfmt ");
        //    wr.Write(16);
        //    wr.Write((ushort)1);
        //    wr.Write(numchannels);
        //    wr.Write(samplerate);
        //    wr.Write(samplerate * samplelength * numchannels);
        //    wr.Write(samplelength * numchannels);
        //    wr.Write((ushort)(8 * samplelength));
        //    wr.Write("data");
        //    wr.Write(numsamples * samplelength);

        //    // for now, just a square wave
        //    Waveform a = new Waveform(440, 50);

        //    double t = 0.0;
        //    for (int i = 0; i < numsamples; i++, t += 1.0 / samplerate)
        //    {
        //        wr.Write((byte)((a.sample(t) + (samplelength == 1 ? 128 : 0)) & 0xff));
        //    }
        //}



    }
}
