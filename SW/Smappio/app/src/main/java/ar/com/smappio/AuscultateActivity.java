package ar.com.smappio;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.media.audiofx.Visualizer;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.os.Environment;
import android.support.design.widget.FloatingActionButton;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.WindowManager;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.InputStream;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.ShortBuffer;
import java.text.SimpleDateFormat;
import java.util.Date;

public class AuscultateActivity extends AppCompatActivity {

    // TODO: Cambiar por la ip obtenida del dispositivo
    private static final String HOST = "192.168.1.2";
    private static final int PORT = 80;
    private Socket socket;

    private Thread thread;
    private AudioTrack audioTrack;
    private int minBufferSize;
    private boolean isPlaying;
    private boolean isAuscultating = false;
    private FloatingActionButton streamButton;
    private WifiManager wifiManager;
    private WifiInfo wifiInfo;
    private Visualizer visualizer;
    private EqualizerView equalizerView;
    private ProgressBar progressBarTimer;
    private TextView countUpTimer;

    private static final float maxSampleValue = 131072; // 2^17 (el bit 18 se usa para el signo)
    private static int playingLength = 345; // Cantidad de bytes en PCM24 a pasar al reproductor por vez
    private static int prebufferingSize = 4; // Cantidad de "playingLength" bytes que se prebufferean
    private int prebufferingCounter = 0; // Cantidad actual de "playingLength" bytes que hay en el buffer. "-1" si no esta en etapade prebuffereo
    private byte[] bufferAux = new byte[playingLength * 2]; // Buffer en el que se almacenan los bytes extraidos del socket
    private int readedAux = 0; // Bytes leidos del socket cargados en bufferAux
    private ByteArrayOutputStream bufferWav = new ByteArrayOutputStream();


    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_auscultate);

        //Flecha de la toolbar para volver al activity anterior
        if (getSupportActionBar() != null){
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setDisplayShowHomeEnabled(true);
        }

        wifiManager = (WifiManager) getApplicationContext().getSystemService(Context.WIFI_SERVICE);
        wifiInfo = wifiManager.getConnectionInfo();

        minBufferSize = AudioTrack.getMinBufferSize(Constant.SAMPLE_RATE, Constant.CHANNEL_CONFIG, Constant.AUDIO_ENCODING);
        audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, Constant.SAMPLE_RATE, Constant.CHANNEL_CONFIG, Constant.AUDIO_ENCODING, minBufferSize * 3, AudioTrack.MODE_STREAM);
        audioTrack.setVolume(1.0f);

        equalizerView = (EqualizerView) findViewById(R.id.equalizer);
        setupEqualizer();

        progressBarTimer = (ProgressBar) findViewById(R.id.progress_bar_timer);
        countUpTimer = (TextView) findViewById(R.id.count_up_timer);
        streamButton = (FloatingActionButton) findViewById(R.id.stream_btn);
        streamButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                //TODO: Esto es temporal, para el floating button
                if(isAuscultating) {
                    stopAuscultate();
                } else {
                    auscultate();
                }
            }
        });
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        int id = item.getItemId();
        if (id == android.R.id.home) {
            finish();
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onPause() {
        super.onPause();
        getWindow().clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        unregisterReceiver(broadcastReceiver);
        stopAuscultate();
    }

    @Override
    protected void onResume() {
        super.onResume();
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        IntentFilter intentFilter = new IntentFilter();
//        intentFilter.addAction(WifiManager.WIFI_STATE_CHANGED_ACTION);
        intentFilter.addAction(WifiManager.NETWORK_STATE_CHANGED_ACTION);
        registerReceiver(broadcastReceiver, intentFilter);
    }

    private BroadcastReceiver broadcastReceiver = new BroadcastReceiver() {

        @Override
        public void onReceive(Context context, Intent intent) {

            String action = intent.getAction();

            if (action != null && !action.isEmpty()) {
//                if(action.equals(WifiManager.WIFI_STATE_CHANGED_ACTION)) {
//                    if(!wifiManager.isWifiEnabled()) {
//                        buildAlertMessageDisconnected();
//                    }
//                }
                if(action.equals(WifiManager.NETWORK_STATE_CHANGED_ACTION)) {
                    WifiInfo newWifiInfo = wifiManager.getConnectionInfo();
                    if(!wifiInfo.getBSSID().equals(newWifiInfo.getBSSID())) {
                        buildAlertMessageDisconnected();
                    }
                }
            }
        }
    };

    private void buildAlertMessageDisconnected() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setMessage("Se desconectó el dispositivo. Por favor, conéctelo nuevamente.")
                .setCancelable(false)
                .setPositiveButton("Aceptar", new DialogInterface.OnClickListener() {
                    public void onClick(final DialogInterface dialog, final int id) {
                        finish();
                    }
                });
        AlertDialog alert = builder.create();
        alert.show();
    }

    public void auscultate() {
        streamButton.setImageResource(R.drawable.ic_stop);
        isAuscultating = true;
        isPlaying = true;
        thread = new Thread(runnable);
        thread.start();
        startCountUp();
    }

    public void stopAuscultate() {
        stopCountUp();
        buildAlertMessageSaveWav();
        streamButton.setImageResource(R.drawable.ic_heart);
        isPlaying = false;
        isAuscultating = false;
    }

    long intervalSeconds = 1;
    CountDownTimer timer = new CountDownTimer(Constant.AUSCULTATE_MAX_TIME * 1000, intervalSeconds * 1000) {
        public void onTick(long millisUntilFinished) {
            SimpleDateFormat sdf = new SimpleDateFormat("mm:ss");
            Date date = new Date();
            date.setTime((Constant.AUSCULTATE_MAX_TIME * 1000) - millisUntilFinished);
            countUpTimer.setText(sdf.format(date));
            long progress = date.getTime() / 1000;
            progressBarTimer.setProgress((int) progress);
        }
        public void onFinish() {
            stopAuscultate();
        }
    };

    private void startCountUp() {
        countUpTimer.setVisibility(View.VISIBLE);
        progressBarTimer.setVisibility(View.VISIBLE);
        timer.start();
    }

    private void stopCountUp() {
        timer.cancel();
        countUpTimer.setText("00:00");
        countUpTimer.setVisibility(View.INVISIBLE);
        progressBarTimer.setProgress(0);
        progressBarTimer.setVisibility(View.INVISIBLE);
    }

    private void buildAlertMessageSaveWav() {
        if(isAuscultating) {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.setTitle("¡Alerta!")
                    .setMessage("¿Desea guardar el audio capturado?")
                    .setCancelable(true)
                    .setPositiveButton("Aceptar", new DialogInterface.OnClickListener() {
                        public void onClick(final DialogInterface dialog, final int id) {
                            buildAlertSaveWav();
                        }
                    })
                    .setNegativeButton("Cancelar", new DialogInterface.OnClickListener() {
                        public void onClick(final DialogInterface dialog, final int id) {
                            dialog.cancel();
                        }
                    });
            AlertDialog alert = builder.create();
            alert.show();
        }
    }

    private void buildAlertSaveWav() {
        LayoutInflater layoutInflater = LayoutInflater.from(AuscultateActivity.this);
        View popupSaveFileView = layoutInflater.inflate(R.layout.popup_save_file, null);
        EditText userInput = (EditText) popupSaveFileView.findViewById(R.id.file_name);;
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setView(popupSaveFileView);
        AlertDialog dialog = builder.setTitle("Guardar audio")
                .setMessage("Ingrese el nombre del paciente.")
                .setCancelable(true)
                .setPositiveButton("Aceptar", new DialogInterface.OnClickListener() {
                    public void onClick(final DialogInterface dialog, final int id) {
                        if (userInput.getText() != null && !userInput.getText().toString().isEmpty()) {
                            String nameFile = userInput.getText().toString();
                            String filePath = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).toString() + File.separator + "Smappio" + File.separator + nameFile + ".wav";
                            try {
                                File savedWav = FileUtils.rawToWave(bufferWav.toByteArray(), filePath);
                                if(savedWav != null) {
                                    Toast.makeText(AuscultateActivity.this, "Se guardó el archivo correctamente", Toast.LENGTH_LONG).show();
                                }
                            } catch (Exception e) {
                                e.printStackTrace();
                            }
                        }
                    }
                })
                .setNegativeButton("Cancelar", new DialogInterface.OnClickListener() {
                    public void onClick(final DialogInterface dialog, final int id) {
                        dialog.cancel();
                    }
                })
                .create();

        userInput.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }
            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
            }
            @Override
            public void afterTextChanged(Editable s) {
                if (userInput.getText() != null && !userInput.getText().toString().isEmpty()){
                    dialog.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(true);
                } else {
                    dialog.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(false);
                }
            }
        });

        dialog.show();
        dialog.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(false);
    }

    private void setupEqualizer() {
        visualizer = new Visualizer(audioTrack.getAudioSessionId());
        visualizer.setCaptureSize(Visualizer.getCaptureSizeRange()[1]);
        visualizer.setDataCaptureListener(new Visualizer.OnDataCaptureListener() {
            public void onWaveFormDataCapture(Visualizer visualizer, byte[] bytes, int samplingRate) {
                equalizerView.updateVisualizer(bytes);
            }

            public void onFftDataCapture(Visualizer visualizer, byte[] bytes, int samplingRate) {

            }
        }, Visualizer.getMaxCaptureRate() / 2, true, false);

        visualizer.setEnabled(true);
    }

    Runnable runnable = new Runnable() {
        @Override
        public void run() {
            try {
                socket = new Socket(HOST, PORT);
                socket.setTcpNoDelay(true);
            } catch (Exception e) {
                e.printStackTrace();
            }

            while (isPlaying) {

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

                float[] bufferForPlaying = getBufferForPlayingPCMFloat(errorFreeBuffer);
                audioTrack.write(bufferForPlaying, 0, bufferForPlaying.length, AudioTrack.WRITE_NON_BLOCKING);

//                short[] bufferForPlaying = getBufferForPlayingPCM16(errorFreeBuffer);
//                audioTrack.write(bufferForPlaying, 0, bufferForPlaying.length);

                playIfNeccesary();
            }

            audioTrack.pause();
            audioTrack.flush();
            prebufferingCounter = 0;

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
    private void playIfNeccesary() {
        if (prebufferingCounter > prebufferingSize) {
            audioTrack.play();
            prebufferingCounter = -1; // Se sale de la etapa de prebuffereo
        } else if (prebufferingCounter != -1) {
            prebufferingCounter++;
        }
    }

    private float[] getBufferForPlayingPCMFloat(byte[] buffer) {
        float[] returnedArray = new float[playingLength / 3];
        for (int i = 0; i < buffer.length; i = i + 3) {
            byte signBit = (byte) ((buffer[i + 2] >> 1) & 1);

            int intValue = ((buffer[i] & 0xFF) << 0)
                    | ((buffer[i + 1] & 0xFF) << 8)
                    | ((buffer[i + 2] & 0xFF) << 16)
                    | (signBit == 1 ? 0xFF : 0x00) << 24; // Relleno 1s;

            loadWavBuffer(intValue);

            float floatValue = intValue / maxSampleValue;

            returnedArray[i / 3] = floatValue;
        }
        return returnedArray;
    }

    private short[] getBufferForPlayingPCM16(byte[] buffer) {
        short[] returnedArray = new short[playingLength / 3];
        for (int i = 0; i < buffer.length; i = i + 3) {
            byte signBit = (byte) ((buffer[i + 2] >> 1) & 1);

            int intValue = ((buffer[i] & 0xFF) << 0)
                    | ((buffer[i + 1] & 0xFF) << 8)
                    | ((buffer[i + 2] & 0xFF) << 16)
                    | (signBit == 1 ? 0xFF : 0x00) << 24; // Relleno 1s;


            short shortValue = (short) (intValue / 4);

            loadWavBuffer(shortValue);

            returnedArray[i / 3] = shortValue;
        }
        return returnedArray;
    }

    private void loadWavBuffer(int intValue) {
        short shortValue = (short) (intValue / 4);
        try {
            ByteBuffer byteBuffer = ByteBuffer.allocate(2);
            byteBuffer.order(ByteOrder.LITTLE_ENDIAN);
            byteBuffer.putShort(shortValue);
            byte[] result = byteBuffer.array();
            bufferWav.write(result);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    private byte[] controlAlgorithm() {
        // Verificar que lo que se lee cumpla con la secuencia 01, 10, 11
        byte[] errorFreeBuffer = new byte[playingLength];
        int i = 0;
        int acumDiscardedBytes = 0;
        while (i < readedAux - 2) {
            int firstByteSeqNumber = (bufferAux[i] >> 6) & 3;
            int secondByteSeqNumber = (bufferAux[i + 1] >> 6) & 3;
            int thirdByteSeqNumber = (bufferAux[i + 2] >> 6) & 3;
            int discardedBytes = 0;

            // Si algun numero no no sigue la secuencia, se descartan bytes para atras, nunca para delante
            if (firstByteSeqNumber != 1) {
                discardedBytes += 1;
            } else if (secondByteSeqNumber != 2) {
                if (secondByteSeqNumber == 1)
                    discardedBytes += 1;
                else if (secondByteSeqNumber == 3)
                    discardedBytes += 2;
            } else if (thirdByteSeqNumber != 3) {
                if (thirdByteSeqNumber == 1)
                    discardedBytes += 2;
                else if (thirdByteSeqNumber == 2)
                    discardedBytes += 3;
            } else {
                // Se vuelven a armar las muestras originales
                int sample = 0;
                byte[] sampleAsByteArray = new byte[4];
                int errorFreeBaseIndex = i - acumDiscardedBytes;
                byte auxByteMSB = 0;    // Most Significant Bits

                // Byte 1 => ultimos 6 bits del primer byte + 2 últimos bits del segundo byte
                auxByteMSB = (byte) ((bufferAux[i + 1] & 3) << 6);                           // 'XX|000000'
                sampleAsByteArray[0] = (byte) (auxByteMSB | (bufferAux[i] & 63));            // 'XX|YYYYYY'

                // Byte 2 => 4 bits del medio del segundo byte + 4 úlitmos bits del último byte
                auxByteMSB = (byte) ((bufferAux[i + 2] & 15) << 4);                          // 'XXXX|0000'
                sampleAsByteArray[1] = (byte) (auxByteMSB | ((bufferAux[i + 1] >> 2) & 15)); // 'XXXX|YYYY'

                // Byte 3 => 1 bit (el 4to de izq a derecha)
                sampleAsByteArray[2] = (byte) ((bufferAux[i + 2] >> 4) & 1);                 // '0000000|X'

                // Byte 3 => 5 bits para el signo(depende del 3ero de izq a derecha)
                // Si el bit mas significativo del samlpe es '1' quiere decir que el numero es negativo, entonces se
                // agrega un padding a la izquierda de '7 + 8' unos, caso contrario, se deja el padding 0 que ya habia y se agregan '8' ceros mas
                byte signBit = (byte) ((bufferAux[i + 2] >> 5) & 1);
                if (signBit == 1) {
                    sampleAsByteArray[2] = (byte) (sampleAsByteArray[2] | 254);              // '1111111|X'
                    sampleAsByteArray[3] = (byte) 255;                                             // '11111111'
                } else {
                    sampleAsByteArray[3] = 0;                                               // '00000000'
                }

                errorFreeBuffer[errorFreeBaseIndex] = sampleAsByteArray[0];
                errorFreeBuffer[errorFreeBaseIndex + 1] = sampleAsByteArray[1];
                errorFreeBuffer[errorFreeBaseIndex + 2] = sampleAsByteArray[2];
            }

            if (discardedBytes == 0)
                i += 3;
            else {
                i += discardedBytes;
                acumDiscardedBytes += discardedBytes;
                readExtraBytes(discardedBytes);
            }
        }

        return errorFreeBuffer;
    }

    private void readExtraBytes(int size) {
        try {
            InputStream is = socket.getInputStream();
            while (is.available() < size) {
                // do nothing
            }
            readedAux += is.read(bufferAux, readedAux, size);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

}