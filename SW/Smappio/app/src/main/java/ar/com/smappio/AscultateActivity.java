package ar.com.smappio;

import android.support.design.widget.FloatingActionButton;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.MenuItem;
import android.view.View;

public class AscultateActivity extends AppCompatActivity {

    private FloatingActionButton streamButton;
    private boolean isAuscultating = false;
    private PCMSocket pcmSocket;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_stream);

        //Flecha de la toolbar para volver al activity anterior
        if (getSupportActionBar() != null){
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setDisplayShowHomeEnabled(true);
        }

        pcmSocket = new PCMSocket();

        streamButton = findViewById(R.id.stream_btn);
        streamButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                //TODO: Esto es temporal, para el floating button
                if(isAuscultating) {
                    streamButton.setImageResource(R.drawable.ic_heart_red_24dp);
                    isAuscultating = false;
                    pcmSocket.stopAuscultate();
                } else {
                    streamButton.setImageResource(R.drawable.ic_stop_black_24dp);
                    isAuscultating = true;
                    pcmSocket.auscultate();
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

}