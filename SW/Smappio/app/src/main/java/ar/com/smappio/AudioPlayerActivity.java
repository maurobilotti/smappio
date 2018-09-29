package ar.com.smappio;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.media.AudioManager;
import android.media.MediaPlayer;
import android.media.audiofx.Visualizer;
import android.net.Uri;
import android.os.Build;
import android.os.Handler;
import android.os.Message;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.ImageButton;
import android.widget.SeekBar;
import android.widget.TextView;

import java.io.File;
import java.net.URISyntaxException;

public class AudioPlayerActivity extends AppCompatActivity {

    //Variables del reproductor
    private ImageButton playBtn;
    private SeekBar positionBar;
    private TextView elapsedTimeLabel;
    private TextView remainingTimeLabel;
    private MediaPlayer mediaPlayer;
    private int totalTime;

    //Variables del file system
    private Uri currentFileURI;

    //Variables del fonocardiograma
    private EqualizerView equalizerView;
    private Visualizer visualizer;
    private WaveformView waveformView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_audio_player);

        playBtn = findViewById(R.id.playBtn);
        elapsedTimeLabel = findViewById(R.id.elapsedTimeLabel);
        remainingTimeLabel = findViewById(R.id.remainingTimeLabel);
        equalizerView = findViewById(R.id.equalizer);
        waveformView = findViewById(R.id.waveform);

        //Flecha de la toolbar para volver al activity anterior
        if (getSupportActionBar() != null){
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setDisplayShowHomeEnabled(true);
        }

        //Obtener parametro enviado desde el activity anterior
        Bundle extras = getIntent().getExtras();
        if(extras != null) {
            currentFileURI = (Uri) extras.get("currentFileURI");
            stopAndResetAudioPlayer();
            startAudioPlayer();
        }

    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_audio_player, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        int id = item.getItemId();
        if (id == R.id.action_share) {
            shareFile();
            return true;
        }
        if (item.getItemId() == android.R.id.home) {
            finish();
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onPause() {
        super.onPause();
        if (isFinishing() && mediaPlayer != null) {
            visualizer.release();
            mediaPlayer.release();
            mediaPlayer = null;
        }
    }

    public void stopAndResetAudioPlayer(){
        if(mediaPlayer != null) {
            mediaPlayer.stop();
            mediaPlayer.reset();
        }
    }

    public void startAudioPlayer() {
        setVolumeControlStream(AudioManager.STREAM_MUSIC);
        mediaPlayer = MediaPlayer.create(this, currentFileURI);
        mediaPlayer.setLooping(false);
        mediaPlayer.seekTo(0);

        totalTime = mediaPlayer.getDuration();

        // Position Bar
        positionBar = findViewById(R.id.positionBar);
        positionBar.setMax(totalTime);
        positionBar.setOnSeekBarChangeListener(
            new SeekBar.OnSeekBarChangeListener() {
                @Override
                public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
                    if (fromUser) {
                        mediaPlayer.seekTo(progress);
                        positionBar.setProgress(progress);
                    }
                }
                @Override
                public void onStartTrackingTouch(SeekBar seekBar) { }
                @Override
                public void onStopTrackingTouch(SeekBar seekBar) {  }
            }
        );

        // Thread (Update positionBar & timeLabel)
        new Thread(new Runnable() {
            @Override
            public void run() {
                while (mediaPlayer != null) {
                    try {
                        Message msg = new Message();
                        msg.what = mediaPlayer.getCurrentPosition();
                        handler.sendMessage(msg);
                        Thread.sleep(1000);
                    } catch (InterruptedException e) { }
                }
            }
        }).start();

        setupEqualizer();
        //setupWaveform();
        mediaPlayer.start();

        playBtn.setBackgroundResource(R.drawable.ic_pause_black_24dp);
    }

    private Handler handler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            int currentPosition = msg.what;
            // Update positionBar.
            positionBar.setProgress(currentPosition);
            // Update Labels.
            String elapsedTime = createTimeLabel(currentPosition);
            elapsedTimeLabel.setText(elapsedTime);
            String remainingTime = createTimeLabel(totalTime-currentPosition);
            remainingTimeLabel.setText("- " + remainingTime);
        }
    };

    public String createTimeLabel(int time) {
        String timeLabel = "";
        int min = time / 1000 / 60;
        int sec = time / 1000 % 60;
        timeLabel = min + ":";
        if (sec < 10) {
            timeLabel += "0";
        }
        timeLabel += sec;
        return timeLabel;
    }

    public void playBtnClick(View view) {
        if (!mediaPlayer.isPlaying()) {
            mediaPlayer.start();
            playBtn.setBackgroundResource(R.drawable.ic_pause_black_24dp);
        } else {
            mediaPlayer.pause();
            playBtn.setBackgroundResource(R.drawable.ic_play_arrow_black_24dp);
        }
    }

    public void shareFile() {
        if(currentFileURI != null){
            Intent intent = new Intent(Intent.ACTION_SEND);
            intent.setType("audio/*");
            intent.putExtra(Intent.EXTRA_STREAM, currentFileURI);
            startActivity(Intent.createChooser(intent, "Compartir archivo de audio"));
        }
    }

    private void setupEqualizer() {
        visualizer = new Visualizer(mediaPlayer.getAudioSessionId());
        visualizer.setCaptureSize(Visualizer.getCaptureSizeRange()[1]);
        visualizer.setDataCaptureListener(new Visualizer.OnDataCaptureListener() {
            public void onWaveFormDataCapture(Visualizer visualizer, byte[] bytes, int samplingRate) {
                equalizerView.updateVisualizer(bytes);
            }
            public void onFftDataCapture(Visualizer visualizer, byte[] bytes, int samplingRate) {

            }
        }, Visualizer.getMaxCaptureRate() / 2, true, false);

        visualizer.setEnabled(true);

        mediaPlayer.setOnCompletionListener(new MediaPlayer.OnCompletionListener() {
            public void onCompletion(MediaPlayer mediaPlayer) {
                visualizer.setEnabled(false);
            }
        });
    }

//    public void setupWaveform() {
//        try {
//            String filePath = FileUtils.getFilePath(this,currentFileURI);
//            File file = new File(filePath);
//            updateVisualizer(FileUtils.fileToBytes(file));
//        } catch (URISyntaxException e) {
//            e.printStackTrace();
//        }
//    }

//    public void updateVisualizer(byte[] bytes) {
//        waveformView.updateVisualizer(bytes);
//    }
//    public void updatePlayerProgress(float percent) {
//        waveformView.updatePlayerPercent(percent);
//    }
}
