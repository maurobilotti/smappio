using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smappio_SEAR
{
    public class PCMHelper
    {
        public PCMHelper(PCMAudioFormat format)
        {
            this._format = format;
        }

        private PCMAudioFormat _format { get; set; }

        public byte[] GetBufferForPlaying(byte[] buffer)
        {
            if (_format == PCMAudioFormat.PCM_32_Float)
                return this.PCM32(buffer);
            if (_format == PCMAudioFormat.PCM_16)
                return this.PCM16(buffer);

            //PCM-24 retorna el buffer intacto
            return buffer;
        }

        private byte[] PCM16(byte[] buffer)
        {
            var returnedList = new List<byte>();
            for (int i = 0; i < buffer.Length; i = i + 3)
            {
                byte signBit = (byte)((buffer[i + 2] >> 1) & 1);

                int asInt = 0;
                if (signBit == 1)
                {
                    asInt = ((buffer[i] & 0xFF) << 0)
                            | ((buffer[i + 1] & 0xFF) << 8)
                            | ((buffer[i + 2] & 0xFF) << 16)
                            | 0xFF << 24; // Relleno 1s
                }
                else
                {
                    asInt = ((buffer[i] & 0xFF) << 0)
                        | ((buffer[i + 1] & 0xFF) << 8)
                        | ((buffer[i + 2] & 0xFF) << 16)
                        | 0x00 << 24; // Relleno 0s
                }

                var intResult = BitConverter.GetBytes(asInt);

                int value = asInt / 4; //https://en.wikipedia.org/wiki/Single-precision_floating-point_format
                
                var numBytes = BitConverter.GetBytes(value);
                //var hexValue = value.ToHexString();

                returnedList.AddRange(numBytes.Take(2));
            }

            return returnedList.ToArray();
        }

        private byte[] PCM32(byte[] buffer)
        {
            var returnedList = new List<byte>();
            for (int i = 0; i < buffer.Length; i = i + 3)
            {
                byte signBit = (byte)((buffer[i + 2] >> 1) & 1);

                int intValue = ((buffer[i] & 0xFF) << 0)
                            | ((buffer[i + 1] & 0xFF) << 8)
                            | ((buffer[i + 2] & 0xFF) << 16)
                            | (signBit == 1 ? 0xFF : 0x00) << 24; // Relleno 1s;               

                var intResult = BitConverter.GetBytes(intValue);

                float floatValue = intValue / (float)131072;

                if (floatValue > 1 || floatValue < -1)
                    throw new IndexOutOfRangeException("Fuera del margen -1 a 1");                

                var floatResult = BitConverter.GetBytes(floatValue);
#if DEBUG
                var hexValue = floatValue.ToHexString();
#endif          
                returnedList.AddRange(floatResult);
            }

            return returnedList.ToArray();
        }
    }
}
