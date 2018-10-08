package ar.com.smappio;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.location.LocationManager;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.support.design.widget.FloatingActionButton;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.MenuItem;
import android.view.View;

public class AscultateActivity extends AppCompatActivity {

    private FloatingActionButton streamButton;
    private boolean isAuscultating = false;
    private PCMSocket pcmSocket;
    private WifiManager wifiManager;
    private WifiInfo wifiInfo;

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

    @Override
    protected void onPause() {
        super.onPause();
        unregisterReceiver(broadcastReceiver);
        streamButton.setImageResource(R.drawable.ic_heart_red_24dp);
        isAuscultating = false;
        pcmSocket.stopAuscultate();
    }

    @Override
    protected void onResume() {
        super.onResume();
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
                        dialog.dismiss();
                        finish();
                    }
                });
        AlertDialog alert = builder.create();
        alert.show();
    }
}