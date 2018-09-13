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

        public static float[] FloatArrayFromByteArray(byte[] input)
        {
            float[] output = new float[input.Length / 4];
            Buffer.BlockCopy(input, 0, output, 0, input.Length);
            return output;
        }

        public static void DrawNormalizedAudio(ref byte[] byteArray, PictureBox pb, Color color)
        {
            float[] data = FloatArrayFromByteArray(byteArray);
            Bitmap bmp;
            if (pb.Image == null)
            {
                bmp = new Bitmap(pb.Width, pb.Height);
            }
            else
            {
                bmp = (Bitmap)pb.Image;
            }

            lock (bmp)
            {
                int BORDER_WIDTH = 5;
                int width = bmp.Width - (2 * BORDER_WIDTH);
                int height = bmp.Height - (2 * BORDER_WIDTH);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Black);
                    Pen pen = new Pen(color);
                    int size = data.Length;
                    for (int iPixel = 0; iPixel < width; iPixel++)
                    {
                        // determine start and end points within WAV
                        int start = (int)((float)iPixel * ((float)size / (float)width));
                        int end = (int)((float)(iPixel + 1) * ((float)size / (float)width));
                        float min = float.MaxValue;
                        float max = float.MinValue;
                        for (int i = start; i < end; i++)
                        {
                            float val = data[i];
                            min = val < min ? val : min;
                            max = val > max ? val : max;
                        }
                        int yMax = BORDER_WIDTH + height - (int)((max + 1) * .5 * height);
                        int yMin = BORDER_WIDTH + height - (int)((min + 1) * .5 * height);
                        g.DrawLine(pen, iPixel + BORDER_WIDTH, yMax,
                            iPixel + BORDER_WIDTH, yMin);
                    }
                }
                pb.Image = bmp;
            }
        }
    }
}
