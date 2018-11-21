package ar.com.smappio;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.media.audiofx.Visualizer;
import android.net.Uri;
import android.net.wifi.WifiManager;
import android.os.Build;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.support.v4.content.FileProvider;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.WindowManager;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.InputStream;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.text.SimpleDateFormat;
import java.util.Date;

import ar.com.smappio.player.AudioWavePlayerActivity;

public class AuscultateActivity extends AppCompatActivity {

    private Socket socket;
    private Thread thread;
    private AudioTrack audioTrack;
    private boolean isPlaying;
    private WifiManager wifiManager;
    private String bssid;
    private boolean firstAuscultate = true;
    private boolean fixBroadcastReceiver = true;

    // Views
    private Visualizer visualizer;
    private EqualizerView equalizerView;
    private ProgressBar progressBarTimer;
    private TextView countUpTimer;
    private ImageButton startAuscultateBtn;
    private ImageButton stopAuscultateBtn;
    private ImageButton playAuscultateBtn;
    private MenuItem shareItem;
    private AlertDialog alertDialogDisconnected;
    private AlertDialog alertDialogSaveFile;
    private AlertDialog alertDialogSaveWav;

    private static final float maxSampleValue = 131072; // 2^17 (el bit 18 se usa para el signo)
    private static int playingLength = 345; // Cantidad de bytes en PCM24 a pasar al reproductor por vez
    private static int prebufferingSize = 4; // Cantidad de "playingLength" bytes que se prebufferean
    private int prebufferingCounter = 0; // Cantidad actual de "playingLength" bytes que hay en el buffer. "-1" si no esta en etapade prebuffereo
    private byte[] bufferAux = new byte[playingLength * 2]; // Buffer en el que se almacenan los bytes extraidos del socket
    private int readedAux = 0; // Bytes leidos del socket cargados en bufferAux
    private ByteArrayOutputStream bufferWav;
    private File currentFile;


    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_auscultate);

        //Flecha de la toolbar para volver al activity anterior
        if (getSupportActionBar() != null){
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setDisplayShowHomeEnabled(true);
        }

        progressBarTimer = findViewById(R.id.progress_bar_timer);
        countUpTimer = findViewById(R.id.count_up_timer);
        startAuscultateBtn = findViewById(R.id.start_auscultate_btn);
        stopAuscultateBtn = findViewById(R.id.stop_auscultate_btn);
        playAuscultateBtn = findViewById(R.id.play_auscultate_btn);
        equalizerView = findViewById(R.id.equalizer);

        wifiManager = (WifiManager) getApplicationContext().getSystemService(Context.WIFI_SERVICE);
        bssid = wifiManager.getConnectionInfo().getBSSID();

        int minBufferSize = AudioTrack.getMinBufferSize(Constant.SAMPLE_RATE, Constant.CHANNEL_CONFIG, Constant.AUDIO_ENCODING);
        audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, Constant.SAMPLE_RATE, Constant.CHANNEL_CONFIG, Constant.AUDIO_ENCODING, minBufferSize * 3, AudioTrack.MODE_STREAM);
        audioTrack.setVolume(1.0f);

        setupEqualizer();

    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_auscultate, menu);
        shareItem = menu.findItem(R.id.action_share);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        int id = item.getItemId();
        if (id == android.R.id.home) {
            isPlaying = false;
            finish();
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onPause() {
        super.onPause();
        getWindow().clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        unregisterReceiver(broadcastReceiver);
        stopAuscultate(stopAuscultateBtn);
    }

    @Override
    protected void onStart() {
        super.onStart();
        if(firstAuscultate) {
            firstAuscultate = false;
            auscultate(startAuscultateBtn);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        IntentFilter intentFilter = new IntentFilter();
        intentFilter.addAction(WifiManager.NETWORK_STATE_CHANGED_ACTION);
        registerReceiver(broadcastReceiver, intentFilter);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        audioTrack = null;
        thread.interrupt();
        thread = null;
        runnable = null;
        broadcastReceiver = null;
        if(alertDialogDisconnected != null) {
            alertDialogDisconnected.dismiss();
            alertDialogDisconnected = null;
        }
        if(alertDialogSaveFile != null) {
            alertDialogSaveFile.dismiss();
            alertDialogSaveFile = null;
        }
        if(alertDialogSaveWav != null) {
            alertDialogSaveWav.dismiss();
            alertDialogSaveWav = null;
        }
    }

    private BroadcastReceiver broadcastReceiver = new BroadcastReceiver() {

        @Override
        public void onReceive(Context context, Intent intent) {

            String action = intent.getAction();

            if (action != null && !action.isEmpty()) {
                if(action.equals(WifiManager.NETWORK_STATE_CHANGED_ACTION)) {
                    if(!bssid.equals(wifiManager.getConnectionInfo().getBSSID()) && fixBroadcastReceiver) {
                        fixBroadcastReceiver = false;
                        isPlaying = false;
                        stopAuscultate(stopAuscultateBtn);
                        buildAlertMessageDisconnected();
                    }
                }
            }
        }
    };

    private void buildAlertMessageDisconnected() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setMessage(R.string.msg_desconecto_dispositivo_conectelo)
                .setCancelable(false)
                .setOnDismissListener(new DialogInterface.OnDismissListener() {
                    @Override
                    public void onDismiss(DialogInterface dialog) {
                        finish();
                    }
                })
                .setPositiveButton(R.string.str_aceptar, new DialogInterface.OnClickListener() {
                    public void onClick(final DialogInterface dialog, final int id) {
                        dialog.dismiss();
                    }
                });
        alertDialogDisconnected = builder.create();
        alertDialogDisconnected.show();
    }

    public void auscultate(View view) {
        stopAuscultateBtn.setVisibility(View.VISIBLE);
        startAuscultateBtn.setVisibility(View.GONE);
        playAuscultateBtn.setEnabled(false);
        playAuscultateBtn.setAlpha(0.3f);
        if(shareItem != null) {
            shareItem.setVisible(false);
        }
        isPlaying = true;
        thread = new Thread(runnable);
        thread.start();
        startCountUp();
    }

    public void stopAuscultate(View view) {
        stopCountUp();
        startAuscultateBtn.setVisibility(View.VISIBLE);
        stopAuscultateBtn.setVisibility(View.GONE);
        if(currentFile != null) {
            playAuscultateBtn.setEnabled(true);
            playAuscultateBtn.setAlpha(1.0f);
            if(shareItem != null) {
                shareItem.setVisible(true);
            }
        }
        if(isPlaying) {
            buildAlertMessageSaveWav();
            isPlaying = false;
        }
    }

    long intervalSeconds = 1;
    CountDownTimer timer = new CountDownTimer(Constant.AUSCULTATE_MAX_TIME * 1000, intervalSeconds * 1000) {
        public void onTick(long millisUntilFinished) {
            SimpleDateFormat sdf = new SimpleDateFormat(Constant.FORMAT_MMSS);
            Date date = new Date();
            date.setTime((Constant.AUSCULTATE_MAX_TIME * 1000) - millisUntilFinished);
            countUpTimer.setText(sdf.format(date));
            long progress = date.getTime() / 1000;
            progressBarTimer.setProgress((int) progress);
        }
        public void onFinish() {
            stopAuscultate(null);
        }
    };

    private void startCountUp() {
        countUpTimer.setVisibility(View.VISIBLE);
        progressBarTimer.setVisibility(View.VISIBLE);
        timer.start();
    }

    private void stopCountUp() {
        timer.cancel();
        countUpTimer.setText(R.string.str_00_00);
        countUpTimer.setVisibility(View.INVISIBLE);
        progressBarTimer.setProgress(0);
        progressBarTimer.setVisibility(View.INVISIBLE);
    }

    private void buildAlertMessageSaveWav() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle(R.string.str_alerta)
                .setMessage(R.string.msg_guardar_audio_capturado)
                .setCancelable(false)
                .setPositiveButton(R.string.str_aceptar, new DialogInterface.OnClickListener() {
                    public void onClick(final DialogInterface dialog, final int id) {
                        buildAlertSaveWav();
                    }
                })
                .setNegativeButton(R.string.str_cancelar, new DialogInterface.OnClickListener() {
                    public void onClick(final DialogInterface dialog, final int id) {
                        dialog.dismiss();
                    }
                });
        alertDialogSaveFile = builder.create();
        alertDialogSaveFile.show();
    }

    private void buildAlertSaveWav() {
        new FileChooserDialog(this).setFileListener(new FileChooserDialog.FileSelectedListener() {
            @Override public void fileSelected(final File file) {
                LayoutInflater layoutInflater = LayoutInflater.from(AuscultateActivity.this);
                View popupSaveFileView = layoutInflater.inflate(R.layout.popup_save_file, null);
                final EditText userInput = popupSaveFileView.findViewById(R.id.file_name);
                AlertDialog.Builder builder = new AlertDialog.Builder(AuscultateActivity.this);
                builder.setView(popupSaveFileView);
                alertDialogSaveWav = builder.setTitle(R.string.msg_guardar_audio)
                        .setMessage(R.string.msg_ingrese_nombre_archivo)
                        .setCancelable(false)
                        .setPositiveButton(R.string.str_aceptar, new DialogInterface.OnClickListener() {
                            public void onClick(final DialogInterface dialog, final int id) {
                                if (userInput.getText() != null && !userInput.getText().toString().isEmpty()) {
                                    String nameFile = userInput.getText().toString();
                                    String filePath = file.getPath() + File.separator + nameFile + ".wav";
                                    try {
                                        currentFile = FileUtils.rawToWave(bufferWav.toByteArray(), filePath);
                                        if(currentFile != null) {
                                            Toast.makeText(AuscultateActivity.this, R.string.msg_guardo_archivo_correctamente, Toast.LENGTH_LONG).show();
                                            playAuscultateBtn.setEnabled(true);
                                            playAuscultateBtn.setAlpha(1.0f);
                                            if(shareItem != null) {
                                                shareItem.setVisible(true);
                                            }
                                        }
                                    } catch (Exception e) {
                                        e.printStackTrace();
                                    }
                                }
                            }
                        })
                        .setNegativeButton(R.string.str_cancelar, new DialogInterface.OnClickListener() {
                            public void onClick(final DialogInterface dialog, final int id) {
                                dialog.dismiss();
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
                            alertDialogSaveWav.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(true);
                        } else {
                            alertDialogSaveWav.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(false);
                        }
                    }
                });

                alertDialogSaveWav.show();
                alertDialogSaveWav.getButton(AlertDialog.BUTTON_POSITIVE).setEnabled(false);
            }
        }).showDialog();
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
                socket = new Socket(Constant.HOST, Constant.PORT);
                socket.setTcpNoDelay(true);
                bufferWav = new ByteArrayOutputStream();
            } catch (Exception e) {
                e.printStackTrace();
            }

            if(socket != null) {
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
//                    short[] bufferForPlaying = getBufferForPlayingPCM16(errorFreeBuffer);
//                    audioTrack.write(bufferForPlaying, 0, bufferForPlaying.length);

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

    public void shareFile(MenuItem view) {
        if(currentFile != null) {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
                Intent intent = new Intent(Intent.ACTION_SEND);
                intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
                Uri uri = FileProvider.getUriForFile(this, this.getApplicationContext().getPackageName() + ".provider", currentFile);
                intent.putExtra(Intent.EXTRA_STREAM, uri);
                intent.setType("audio/*");
                startActivity(intent);
            } else {
                Intent intent = new Intent(Intent.ACTION_SEND);
                intent.putExtra(Intent.EXTRA_STREAM, Uri.fromFile(currentFile));
                intent.setType("audio/*");
                startActivity(intent);
            }
        }
    }

    public void playFile(View view) {
        if(currentFile != null) {
            Uri currentFileURI = Uri.fromFile(currentFile);
            Intent intent = new Intent(this, AudioWavePlayerActivity.class);
            intent.putExtra("currentFileURI", currentFileURI);
            startActivity(intent);
        }
    }

}