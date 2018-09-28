package ar.com.smappio;

import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;

import java.io.InputStream;
import java.net.Socket;

public class PCMSocket {

    private static final String HOST = "192.168.1.2";
    private static final int PORT = 80;

    private static final int SAMPLE_RATE = 4600;
    private static final int CHANNEL_CONFIG = AudioFormat.CHANNEL_OUT_MONO;
    private static final int AUDIO_ENCODING = AudioFormat.ENCODING_PCM_FLOAT;
    private AudioTrack audioTrack;
    private boolean isPlaying = true;
    private int minBufferSize;
    private Socket socket;
    private int playingLength = 345;
    private static final int MAX_SAMPLE_VALUE = 131072; // 2^17 (el bit 18 se usa para el signo)

    Thread thread;

    Runnable runnable = new Runnable() {

        @Override
        public void run() {

            byte[] buffer = new byte[playingLength];

            try {
                socket = new Socket(HOST, PORT);
            } catch (Exception e) {
                e.printStackTrace();
            }

            audioTrack.play();

            while(isPlaying) {

                try {
                    InputStream is = socket.getInputStream();
                    if(is.available() < playingLength) {
                        continue;
                    }
                    socket.getInputStream().read(buffer,0, playingLength);
                } catch (Exception e) {
                    e.printStackTrace();
                }

                float[] fbuffer = new float[playingLength / 3];

                for (int i = 0; i < playingLength; i += 3) {
                    byte signBit = (byte)((buffer[i + 2] >> 1) & 1);

                    int asInt = 0;
                    asInt = ((buffer[i] & 0xFF) << 0)
                            | ((buffer[i + 1] & 0xFF) << 8)
                            | ((buffer[i + 2] & 0xFF) << 16);

                    if(signBit == 1) {
                        asInt = asInt | 0xFF000000;
                    }

                    float normalizedFloatSample = (float) asInt / MAX_SAMPLE_VALUE; // Se obtiene un un número entre -1.00 y 1.00

                    int index = i / 3;
                    fbuffer[index] = normalizedFloatSample;

                }

                audioTrack.write(fbuffer, 0, fbuffer.length, AudioTrack.WRITE_NON_BLOCKING);
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
        minBufferSize = AudioTrack.getMinBufferSize(SAMPLE_RATE, CHANNEL_CONFIG, AUDIO_ENCODING);
        audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, SAMPLE_RATE, CHANNEL_CONFIG, AUDIO_ENCODING, minBufferSize * 2, AudioTrack.MODE_STREAM);
        thread = new Thread(runnable);
        thread.start();
    }

    public void stopAuscultate() {
        thread.interrupt();
    }

}