package ar.com.smappio;

import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;

import java.io.InputStream;
import java.net.Socket;
import java.nio.ByteBuffer;

public class PCMSocket {

    private static final String HOST = "192.168.1.2";
    private static final int PORT = 80;

    private static final int SAMPLE_RATE = 4600;
    private static final int CHANNEL_CONFIG = AudioFormat.CHANNEL_OUT_MONO;
    private static final int AUDIO_ENCODING = AudioFormat.ENCODING_PCM_16BIT;
    private AudioTrack audioTrack;
    private boolean isPlaying = true;
    private int playBufferSize;
    private Socket socket;
    private int playingLength = 345;
    private int amplitudeMultiplier = 8000;

    Thread thread;

    Runnable runnable = new Runnable() {

        @Override
        public void run() {

            byte[] buffer = new byte[playingLength * 2];

            try {
                socket = new Socket(HOST, PORT);
            } catch (Exception e) {
                e.printStackTrace();
            }

            audioTrack.play();

            while(isPlaying) {

                int readSize = 0;

                try {
                    InputStream is = socket.getInputStream();
                    if(is.available() < playingLength) {
                        continue;
                    }
                    readSize = socket.getInputStream().read(buffer,0, playingLength);
                } catch (Exception e) {
                    e.printStackTrace();
                }

                byte[] sbuffer = new byte[(readSize * 4) / 3];

                for (int i = 0; i < readSize; i += 3) {
                    byte signBit = (byte)((buffer[i + 2] >> 1) & 1);

                    int asInt = 0;
                    asInt = ((buffer[i] & 0xFF) << 0)
                            | ((buffer[i + 1] & 0xFF) << 8)
                            | ((buffer[i + 2] & 0xFF) << 16);
//                            | ((buffer[i + 3] & 0xFF) << 24);

                    if(signBit == 1) {
                        asInt = asInt | 0xFF000000;
                    }

                    asInt = asInt * amplitudeMultiplier;

                    float asFloat = 0;
                    asFloat = Float.intBitsToFloat(asInt);

                    int baseIndex = (i * 4) / 3;
                    byte[] floatArray = float2ByteArray(asFloat);
                    sbuffer[baseIndex] = floatArray[0];
                    sbuffer[baseIndex + 1] = floatArray[1];
                    sbuffer[baseIndex + 2] = floatArray[2];
                    sbuffer[baseIndex + 3] = floatArray[3];

//                    sbuffer[i] = (byte) i;
                }

                audioTrack.write(sbuffer, 0, (playingLength * 4) / 3);
            }

            audioTrack.stop();

            try {
                socket.close();
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    };

    public void auscultate() {
        playBufferSize = AudioTrack.getMinBufferSize(SAMPLE_RATE, CHANNEL_CONFIG, AUDIO_ENCODING);
        audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, SAMPLE_RATE, CHANNEL_CONFIG, AUDIO_ENCODING, playBufferSize, AudioTrack.MODE_STREAM);
        thread = new Thread(runnable);
        thread.start();
    }

    public void stopAuscultate() {
        thread.interrupt();
    }

    private byte [] float2ByteArray (float value) {
        return ByteBuffer.allocate(4).putFloat(value).array();
    }
}