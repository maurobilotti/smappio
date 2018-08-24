package ar.com.smappio;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.support.constraint.ConstraintLayout;
import android.support.design.widget.NavigationView;
import android.support.v4.view.GravityCompat;
import android.support.v4.widget.DrawerLayout;
import android.support.v7.app.ActionBarDrawerToggle;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.Menu;
import android.view.MenuItem;
import android.media.MediaPlayer;
import android.os.Handler;
import android.os.Message;
import android.view.View;
import android.widget.Button;
import android.widget.SeekBar;
import android.widget.TextView;

public class MainActivity extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {

    public static final int CODE_FILE_SYSTEM = 1001;

    Button playBtn;
    SeekBar positionBar;
    TextView elapsedTimeLabel;
    TextView remainingTimeLabel;
    MediaPlayer mediaPlayer;
    int totalTime;
    Uri currentFileURI;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, R.string.navigation_drawer_open, R.string.navigation_drawer_close);
        drawer.addDrawerListener(toggle);
        toggle.syncState();

        NavigationView navigationView = (NavigationView) findViewById(R.id.nav_view);
        navigationView.setNavigationItemSelectedListener(this);
    }

    @Override
    public void onBackPressed() {
        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        if (drawer.isDrawerOpen(GravityCompat.START)) {
            drawer.closeDrawer(GravityCompat.START);
        } else {
            super.onBackPressed();
        }
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    @SuppressWarnings("StatementWithEmptyBody")
    @Override
    public boolean onNavigationItemSelected(MenuItem item) {
        // Handle navigation view item clicks here.
        int id = item.getItemId();

        if (id == R.id.nav_slideshow) {
            openFileSystem();
        } else if (id == R.id.nav_manage) {

        } else if (id == R.id.nav_share) {

        }

        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        drawer.closeDrawer(GravityCompat.START);
        return true;
    }

    public void openFileSystem(){
        Intent intent = new Intent();
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        intent.setType("audio/*");
        intent.setAction(Intent.ACTION_GET_CONTENT);
        startActivityForResult(Intent.createChooser(intent, "Archivos"), CODE_FILE_SYSTEM);
    }

    public void onActivityResult(int requestCode, int resultCode, Intent data) {

        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == CODE_FILE_SYSTEM) {
            stopAndResetAudioPlayer();
            startAudioPlayer(data);
        }
    }

    public void stopAndResetAudioPlayer(){
        if(mediaPlayer != null) {
            mediaPlayer.stop();
            mediaPlayer.reset();
        }
    }

    public void startAudioPlayer(Intent data) {
        if (data != null) {
            //Mostrar reproductor al elegir el tema
            ConstraintLayout contentMain = (ConstraintLayout) findViewById(R.id.content_main);
            contentMain.setVisibility(ConstraintLayout.VISIBLE);

            currentFileURI = data.getData();

            playBtn = (Button) findViewById(R.id.playBtn);
            elapsedTimeLabel = (TextView) findViewById(R.id.elapsedTimeLabel);
            remainingTimeLabel = (TextView) findViewById(R.id.remainingTimeLabel);

            mediaPlayer = MediaPlayer.create(this, currentFileURI);
            mediaPlayer.setLooping(true);
            mediaPlayer.seekTo(0);
            mediaPlayer.setVolume(0.5f, 0.5f);
            totalTime = mediaPlayer.getDuration();

            // Position Bar
            positionBar = (SeekBar) findViewById(R.id.positionBar);
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

            mediaPlayer.start();
            playBtn.setBackgroundResource(android.R.drawable.ic_media_pause);
        }
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
            playBtn.setBackgroundResource(android.R.drawable.ic_media_pause);
        } else {
            mediaPlayer.pause();
            playBtn.setBackgroundResource(android.R.drawable.ic_media_play);
        }
    }
}
