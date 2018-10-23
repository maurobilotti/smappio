package ar.com.smappio;

import android.content.Intent;
import android.media.AudioManager;
import android.media.MediaPlayer;
import android.media.audiofx.Visualizer;
import android.net.Uri;
import android.os.Handler;
import android.os.Message;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.WindowManager;
import android.widget.ImageButton;
import android.widget.SeekBar;
import android.widget.TextView;

import java.io.File;
import java.net.URISyntaxException;

public class AudioPlayerActivity extends AppCompatActivity {

    //Variables del reproductor
    private ImageButton playBtn;
    private SeekBar positionBar;
    private TextView elapsedTimeLbl;
    private TextView remainingTimeLbl;
    private MediaPlayer mediaPlayer;
    private int totalTime;

    //Variables del file system
    private Uri currentFileURI;

    //Variables del fonocardiograma
    private EqualizerView equalizerView;
    private Visualizer visualizer;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_audio_player);

        playBtn = (ImageButton) findViewById(R.id.play_btn);
        positionBar = (SeekBar) findViewById(R.id.position_bar);
        elapsedTimeLbl = (TextView) findViewById(R.id.elapsed_time_lbl);
        remainingTimeLbl = (TextView) findViewById(R.id.remaining_time_lbl);
        equalizerView = (EqualizerView) findViewById(R.id.equalizer);

        //Flecha de la toolbar para volver al activity anterior
        if (getSupportActionBar() != null){
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setDisplayShowHomeEnabled(true);
        }

        //Obtener parametro enviado desde el activity anterior
        Bundle extras = getIntent().getExtras();
        if(extras != null) {
            currentFileURI = (Uri) extras.get("currentFileURI");
            setupAudioPlayer();
            setupEqualizer();
            //setupWaveform();
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
        getWindow().clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        if (mediaPlayer != null && mediaPlayer.isPlaying()) {
            mediaPlayer.pause();
            playBtn.setBackgroundResource(R.drawable.ic_play);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
//        if (mediaPlayer != null && !mediaPlayer.isPlaying()) {
//            mediaPlayer.start();
//            playBtn.setBackgroundResource(R.drawable.ic_pause);
//        }
    }

    public void setupAudioPlayer() {

        //Se le settea el volumen a la activity segun el volumen de Sonido Multimedia
        setVolumeControlStream(AudioManager.STREAM_MUSIC);

        mediaPlayer = MediaPlayer.create(this, currentFileURI);
        mediaPlayer.setLooping(true);
        mediaPlayer.seekTo(0);
        mediaPlayer.setVolume(1,1);

        totalTime = mediaPlayer.getDuration();

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

        //Actualiza el tiempo de reproduccion
        new Thread(runnable).start();

        mediaPlayer.start();

        playBtn.setBackgroundResource(R.drawable.ic_pause);
    }

    private Runnable runnable = new Runnable() {
        @Override
        public void run() {
            while (mediaPlayer != null) {
                try {
                    Message msg = new Message();
                    msg.what = mediaPlayer.getCurrentPosition();
                    handler.sendMessage(msg);
                    Thread.sleep(1000);
                } catch (InterruptedException e) {
                }
            }
        }
    };

    private Handler handler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            int currentPosition = msg.what;
            // Update positionBar.
            positionBar.setProgress(currentPosition);
            // Update Labels.
            String elapsedTime = createTimeLabel(currentPosition);
            elapsedTimeLbl.setText(elapsedTime);
            String remainingTime = createTimeLabel(totalTime - currentPosition);
            remainingTimeLbl.setText("- " + remainingTime);
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
            playBtn.setBackgroundResource(R.drawable.ic_pause);
        } else {
            mediaPlayer.pause();
            playBtn.setBackgroundResource(R.drawable.ic_play);
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

//        mediaPlayer.setOnCompletionListener(new MediaPlayer.OnCompletionListener() {
//            public void onCompletion(MediaPlayer mediaPlayer) {
//                visualizer.setEnabled(false);
//            }
//        });
    }

}
