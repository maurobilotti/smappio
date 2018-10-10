package ar.com.smappio;

import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;

import java.io.InputStream;
import java.net.Socket;

public class PCMSocket {

    private static final String HOST = "192.168.1.2";
    private static final int PORT = 80;
    private Socket socket;

    private static final int SAMPLE_RATE = 4600;
    private static final int CHANNEL_CONFIG = AudioFormat.CHANNEL_OUT_MONO;
    private static final int AUDIO_ENCODING = AudioFormat.ENCODING_PCM_FLOAT;
    private static final float MAX_SAMPLE_VALUE = 2^17; // 2^17 (el bit 18 se usa para el signo)
    private static int playingLength = 345; // Cantidad de bytes en PCM24 a pasar al reproductor por vez

    private AudioTrack audioTrack;
    private int minBufferSize;
    private boolean isPlaying;
    private static int prebufferingSize = 4; // Cantidad de "playingLength" bytes que se prebufferean
    private int prebufferingCounter = 0; // Cantidad actual de "playingLength" bytes que hay en el buffer. "-1" si no esta en etapade prebuffereo

    byte[] bufferAux = new byte[playingLength * 2]; // Buffer en el que se almacenan los bytes extraidos del socket
    private int readedAux = 0; // Bytes leidos del socket cargados en bufferAux

    Thread thread;

    Runnable runnable = new Runnable() {

        @Override
        public void run() {

            try {
                socket = new Socket(HOST, PORT);
                socket.setTcpNoDelay(true);
            } catch (Exception e) {
                e.printStackTrace();
            }

            while(isPlaying) {

                try {
                    InputStream is = socket.getInputStream();
                    if (is.available() < playingLength) {
                        continue;
                    }
                    readedAux = is.read(bufferAux, 0, playingLength);
                } catch (Exception e) {
                    e.printStackTrace();
                }

                byte[] errorFreeBuffer = controlAlgorithm();

                float[] bufferForPlaying = getBufferForPlaying(errorFreeBuffer);

                audioTrack.write(bufferForPlaying, 0, bufferForPlaying.length, AudioTrack.WRITE_NON_BLOCKING);

                playIfNeccesary();
            }
            audioTrack.stop();

            try {
                socket.close();
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    };

    /**
     * Le da Play la pista de audio solo si se recibieron "prebufferingSize" veces "playinLengt" bytes de datos.
     */
    private void playIfNeccesary()
    {
        if (prebufferingCounter > prebufferingSize) {
            audioTrack.play();
            prebufferingCounter = -1; // Se sale de la etapa de prebuffereo
        }
        else if (prebufferingCounter != -1)
        {
            prebufferingCounter++;
        }
    }

    private float[] getBufferForPlaying(byte[] buffer) {
        float[] returnedArray = new float[playingLength / 3];
        for (int i = 0; i < buffer.length; i = i + 3) {
            byte signBit = (byte)((buffer[i + 2] >> 1) & 1);

            int intValue = ((buffer[i] & 0xFF) << 0)
                    | ((buffer[i + 1] & 0xFF) << 8)
                    | ((buffer[i + 2] & 0xFF) << 16)
                    | (signBit == 1 ? 0xFF : 0x00) << 24; // Relleno 1s;

            float floatValue = intValue / MAX_SAMPLE_VALUE;

            returnedArray[i / 3] = floatValue;
        }
        return returnedArray;
    }

    private byte[] controlAlgorithm()
    {
        // Verificar que lo que se lee cumpla con la secuencia 01, 10, 11
        byte[] errorFreeBuffer = new byte[playingLength];
        int i = 0;
        int acumDiscardedBytes = 0;
        while (i < readedAux - 2)
        {
            int firstByteSeqNumber = (bufferAux[i] >> 6)  & 3;
            int secondByteSeqNumber = (bufferAux[i + 1] >> 6) & 3;
            int thirdByteSeqNumber = (bufferAux[i + 2] >> 6) & 3;
            int discardedBytes = 0;

            // Si algun numero no no sigue la secuencia, se descartan bytes para atras, nunca para delante
            if (firstByteSeqNumber != 1)
            {
                discardedBytes += 1;
            }
            else if (secondByteSeqNumber != 2)
            {
                if (secondByteSeqNumber == 0)
                {
                    discardedBytes += 1;
                }
                else if (secondByteSeqNumber == 1)
                    discardedBytes += 1;
                else if (secondByteSeqNumber == 3)
                    discardedBytes += 2;
            }
            else if (thirdByteSeqNumber != 3)
            {
                if (thirdByteSeqNumber == 0)
                {
                    discardedBytes += 1;
                }
                else if (thirdByteSeqNumber == 1)
                    discardedBytes += 2;
                else if (thirdByteSeqNumber == 2)
                    discardedBytes += 3;
            }
            else
            {
                // Se vuelven a armar las muestras originales
                int sample = 0;
                byte[] sampleAsByteArray = new byte[4];
                int errorFreeBaseIndex = i - acumDiscardedBytes;
                byte auxByteMSB = 0;    // Most Significant Bits

                // Byte 1 => ultimos 6 bits del primer byte + 2 últimos bits del segundo byte
                auxByteMSB = (byte)((bufferAux[i + 1] & 3) << 6);                           // 'XX|000000'
                sampleAsByteArray[0] = (byte)(auxByteMSB | (bufferAux[i] & 63));            // 'XX|YYYYYY'

                // Byte 2 => 4 bits del medio del segundo byte + 4 úlitmos bits del último byte
                auxByteMSB = (byte)((bufferAux[i + 2] & 15) << 4);                          // 'XXXX|0000'
                sampleAsByteArray[1] = (byte)(auxByteMSB | ((bufferAux[i + 1] >> 2) & 15)); // 'XXXX|YYYY'

                // Byte 3 => 1 bit (el 4to de izq a derecha)
                sampleAsByteArray[2] = (byte)((bufferAux[i + 2] >> 4) & 1);                 // '0000000|X'

                // Byte 3 => 5 bits para el signo(depende del 3ero de izq a derecha)
                // Si el bit mas significativo del samlpe es '1' quiere decir que el numero es negativo, entonces se
                // agrega un padding a la izquierda de '7 + 8' unos, caso contrario, se deja el padding 0 que ya habia y se agregan '8' ceros mas
                byte signBit = (byte)((bufferAux[i + 2] >> 5) & 1);
                if (signBit == 1)
                {
                    sampleAsByteArray[2] = (byte)(sampleAsByteArray[2] | 254);              // '1111111|X'
                    sampleAsByteArray[3] = (byte)255;                                             // '11111111'
                }
                else
                {
                    sampleAsByteArray[3] = 0;                                               // '00000000'
                }

                errorFreeBuffer[errorFreeBaseIndex] = sampleAsByteArray[0];
                errorFreeBuffer[errorFreeBaseIndex + 1] = sampleAsByteArray[1];
                errorFreeBuffer[errorFreeBaseIndex + 2] = sampleAsByteArray[2];
            }

            if (discardedBytes == 0)
                i += 3;
            else
            {
                i += discardedBytes;
                acumDiscardedBytes += discardedBytes;
                readExtraBytes(discardedBytes);
            }
        }

        return errorFreeBuffer;
    }

    private void readExtraBytes(int size)
    {

        try {
            InputStream is = socket.getInputStream();
            while(is.available() < size) {
                // do nothing
            }
            readedAux += is.read(bufferAux, readedAux, size);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void auscultate() {
        minBufferSize = AudioTrack.getMinBufferSize(SAMPLE_RATE, CHANNEL_CONFIG, AUDIO_ENCODING);
        audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, SAMPLE_RATE, CHANNEL_CONFIG, AUDIO_ENCODING, minBufferSize * 3, AudioTrack.MODE_STREAM);
        isPlaying = true;
        thread = new Thread(runnable);
        thread.start();
    }

    public void stopAuscultate() {
        isPlaying = false;
    }
}