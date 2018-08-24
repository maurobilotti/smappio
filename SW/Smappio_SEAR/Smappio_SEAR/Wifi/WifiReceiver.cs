using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Smappio_SEAR.Wifi
{
    public abstract class WifiReceiver : Receiver
    {
        protected IDisposable ClientReceiver { get; set; }        
        protected string ipAddress = "192.168.1.2";
        protected int port = 80;
        protected byte[] bufferAux;
        protected int readedAux = 0;   
        
        public override byte[] ControlAlgorithm()
        {
            //Verificar que lo que se lee cumpla con la secuencia 01, 10, 11                    
            byte[] errorFreeBuffer = new byte[_playingLength];
            errorFreeReaded = 0;
            int i = 0;
            int acumDiscardedBytes = 0;
            while (i < readedAux - 3)
            {
                int firstByteSeqNumber = bufferAux[i] >> 6;
                int secondByteSeqNumber = bufferAux[i + 1] >> 6;
                int thirdByteSeqNumber = bufferAux[i + 2] >> 6;
                int discardedBytes = 0;

                // Si algun numero no no sigue la secuencia, se descartan bytes para atras, nunca para delante
                if (firstByteSeqNumber != 1)
                {
                    discardedBytes += 1;
                }
                else if (secondByteSeqNumber != 2)
                {
                    if (secondByteSeqNumber == 1)
                        discardedBytes += 1;
                    else if (secondByteSeqNumber == 3)
                        discardedBytes += 2;
                }
                else if (thirdByteSeqNumber != 3)
                {
                    if (thirdByteSeqNumber == 1)
                        discardedBytes += 2;
                    else if (thirdByteSeqNumber == 2)
                        discardedBytes += 3;
                }
                else
                {
                    //Vuelvo a armar las muetras originales
                    int sample = 0;
                    byte[] sampleAsByteArray = new byte[sizeof(int)];
                    int errorFreeBaseIndex = i - acumDiscardedBytes;
                    byte auxByteMSB = 0;    // Most Significant Bits

                    //Byte 1 => ultimos 6 bits del primer byte + 2 últimos bits del segundo byte
                    auxByteMSB = (byte)((bufferAux[i + 1] & 3) << 6);                           // 'XX|000000'
                    sampleAsByteArray[0] = (byte)(auxByteMSB | (bufferAux[i] & 63));            // 'XX|YYYYYY'

                    //Byte 2 => 4 bits del medio del segundo byte + 4 úlitmos bits del último byte
                    auxByteMSB = (byte)((bufferAux[i + 2] & 15) << 4);                          // 'XXXX|0000'
                    sampleAsByteArray[1] = (byte)(auxByteMSB | ((bufferAux[i + 1] >> 2) & 15)); // 'XXXX|YYYY'

                    //Byte 3 => 1 bit (el 4to de izq a derecha)
                    sampleAsByteArray[2] = (byte)((bufferAux[i + 2] >> 4) & 1);                 // '0000000|X'

                    //Byte 3 => 5 bits para el signo(depende del 3ero de izq a derecha)
                    // Si el bit mas significativo del samlpe es '1' quiere decir que el numero es negativo, entonces se
                    // agrega un padding a la izquierda de '7 + 8' unos, caso contrario, se deja el padding 0 que ya habia y se agregan '8' ceros mas
                    byte signBit = (byte)((bufferAux[i + 2] >> 5) & 1);
                    if (signBit == 1)
                    {
                        sampleAsByteArray[2] = (byte)(sampleAsByteArray[2] | 254);              // '1111111|X'
                        sampleAsByteArray[3] = 255;                                             // '11111111'
                    }
                    else
                    {
                        sampleAsByteArray[3] = 0;                                               // '00000000'
                    }

                    // Se amplifica el sonido multiplicando por la constante '_amplitudeMultiplier'
                    sample = BitConverter.ToInt32(sampleAsByteArray, 0) * _amplitudeMultiplier;
                    sampleAsByteArray = BitConverter.GetBytes(sample);

                    errorFreeBuffer[errorFreeBaseIndex] = sampleAsByteArray[0];
                    errorFreeBuffer[errorFreeBaseIndex + 1] = sampleAsByteArray[1];
                    errorFreeBuffer[errorFreeBaseIndex + 2] = sampleAsByteArray[2];
                    errorFreeReaded += 3;
                }

                if (discardedBytes == 0)
                    i += 3;
                else
                {
                    i += discardedBytes;
                    acumDiscardedBytes += discardedBytes;
                }
            }
            
            return errorFreeBuffer;
        }

        public bool CanConnect()
        {
            Ping x = new Ping();
            PingReply reply = x.Send(IPAddress.Parse(ipAddress));
            return reply.Status != IPStatus.TimedOut && reply.Status != IPStatus.DestinationHostUnreachable;
        }
        
    }
}
