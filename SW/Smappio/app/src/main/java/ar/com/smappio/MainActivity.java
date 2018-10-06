package ar.com.smappio;

import android.Manifest;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.graphics.drawable.Drawable;
import android.net.Uri;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Build;
import android.os.Bundle;
import android.support.design.widget.FloatingActionButton;
import android.support.design.widget.NavigationView;
import android.support.v4.view.GravityCompat;
import android.support.v4.widget.DrawerLayout;
import android.support.v7.app.ActionBarDrawerToggle;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.TextView;

public class MainActivity extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {

    private static final int PERMISSIONS_REQUEST_CODE = 101;

    private WifiManager wifiManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        checkTotalPermission();

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab);
        fab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                openWifiScanActivity();
            }
        });

        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, R.string.navigation_drawer_open, R.string.navigation_drawer_close);
        drawer.addDrawerListener(toggle);
        toggle.syncState();

        NavigationView navigationView = (NavigationView) findViewById(R.id.nav_view);
        navigationView.setNavigationItemSelectedListener(this);

        wifiManager = (WifiManager) getApplicationContext().getSystemService(Context.WIFI_SERVICE);
    }

    //Al presionar el botón físico "volver atrás (<)", si esta abierta la sidebar, se cierra
    @Override
    public void onBackPressed() {
        DrawerLayout drawer = findViewById(R.id.drawer_layout);
        if (drawer.isDrawerOpen(GravityCompat.START)) {
            drawer.closeDrawer(GravityCompat.START);
        } else {
            super.onBackPressed();
        }
    }

    @Override
    protected void onPause() {
        super.onPause();
        unregisterReceiver(wifiReceiver);
    }

    @Override
    protected void onResume() {
        super.onResume();
        registerReceiver(wifiReceiver, new IntentFilter(WifiManager.NETWORK_STATE_CHANGED_ACTION));
//        updateDeviceConnected();
    }

//    @Override
//    public boolean onCreateOptionsMenu(Menu menu) {
//        // Inflate the menu; this adds items to the action bar if it is present.
//        getMenuInflater().inflate(R.menu.main, menu);
//        return true;
//    }

//    @Override
//    public boolean onOptionsItemSelected(MenuItem item) {
//        // Handle action bar item clicks here. The action bar will
//        // automatically handle clicks on the Home/Up button, so long
//        // as you specify a parent activity in AndroidManifest.xml.
//        int id = item.getItemId();
//
//        //noinspection SimplifiableIfStatement
//        if (id == R.id.action_settings) {
//            return true;
//        }
//
//        return super.onOptionsItemSelected(item);
//    }

    @SuppressWarnings("StatementWithEmptyBody")
    @Override
    public boolean onNavigationItemSelected(MenuItem item) {
        // Handle navigation view item clicks here.
        int id = item.getItemId();

        if (id == R.id.nav_slideshow) {
            openFileSystem(Constant.CODE_FILE_SYSTEM_PLAY);
        } else if (id == R.id.nav_manage) {

        } else if (id == R.id.nav_share) {
            openFileSystem(Constant.CODE_FILE_SYSTEM_SHARE);
        }

        DrawerLayout drawer = (DrawerLayout) findViewById(R.id.drawer_layout);
        drawer.closeDrawer(GravityCompat.START);
        return true;
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        if (requestCode == PERMISSIONS_REQUEST_CODE) {
            boolean grantedAllPermission = true;
            for(int i = 0; i < grantResults.length; i++) {
                if (grantResults[i] != PackageManager.PERMISSION_GRANTED) {
                    grantedAllPermission = false;
                }
            }
            if(!grantedAllPermission) {
                AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(this);
                LayoutInflater layoutInflater = LayoutInflater.from(MainActivity.this);
                View popupAlertPermissions = layoutInflater.inflate(R.layout.popup_alert_permissions, null);
                alertDialogBuilder.setView(popupAlertPermissions);
                alertDialogBuilder.setTitle("Alerta");
                alertDialogBuilder
                        .setCancelable(false)
                        .setPositiveButton("Aceptar",
                            new DialogInterface.OnClickListener() {
                                public void onClick(DialogInterface dialog, int id) {
                                    finish();
                                }
                            });
                AlertDialog alertDialog = alertDialogBuilder.create();
                alertDialog.show();
            }
        }
    }

    public void checkTotalPermission() {

        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.M &&
            !(checkSelfPermission(Manifest.permission.READ_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.WRITE_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.READ_PHONE_STATE) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.WAKE_LOCK) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.MODIFY_AUDIO_SETTINGS) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.RECORD_AUDIO) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.ACCESS_WIFI_STATE) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.CHANGE_WIFI_STATE) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.ACCESS_COARSE_LOCATION) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED
            && checkSelfPermission(Manifest.permission.INTERNET) == PackageManager.PERMISSION_GRANTED)){

            requestPermissions(new String[]{
                    Manifest.permission.READ_EXTERNAL_STORAGE,
                    Manifest.permission.WRITE_EXTERNAL_STORAGE,
                    Manifest.permission.READ_PHONE_STATE,
                    Manifest.permission.WAKE_LOCK,
                    Manifest.permission.MODIFY_AUDIO_SETTINGS,
                    Manifest.permission.RECORD_AUDIO,
                    Manifest.permission.ACCESS_WIFI_STATE,
                    Manifest.permission.CHANGE_WIFI_STATE,
                    Manifest.permission.ACCESS_COARSE_LOCATION,
                    Manifest.permission.ACCESS_FINE_LOCATION,
                    Manifest.permission.INTERNET
            }, PERMISSIONS_REQUEST_CODE);

        }
    }

    public void openFileSystem(int code){
        Intent intent = new Intent();
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        intent.setType("audio/*");
        intent.setAction(Intent.ACTION_GET_CONTENT);
        startActivityForResult(Intent.createChooser(intent, "Seleccionar archivo"), code);
    }

    public void openWifiScanActivity() {
        Intent intent = new Intent(this, WifiActivity.class);
        startActivityForResult(intent, Constant.CODE_WIFI_CONNECTED);
    }

    public void onActivityResult(int requestCode, int resultCode, Intent data) {

        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == Constant.CODE_FILE_SYSTEM_PLAY && data != null) {
            Uri currentFileURI = data.getData();

            Intent intent = new Intent(this, AudioPlayerActivity.class);
            intent.putExtra("currentFileURI", currentFileURI);
            startActivity(intent);
        }

        if (requestCode == Constant.CODE_FILE_SYSTEM_SHARE && data != null) {
            Uri currentFileURI = data.getData();

            Intent intent = new Intent(Intent.ACTION_SEND);
            intent.setType("audio/*");
            intent.putExtra(Intent.EXTRA_STREAM, currentFileURI);
            startActivity(Intent.createChooser(intent, "Compartir archivo de audio"));
        }

//        if(requestCode == Constant.CODE_WIFI_CONNECTED && data != null) {
//            int networkId = (int) data.getExtras().get("networkId");
//            String ssid = (String) data.getExtras().get("ssid");
//            TextView deviceConnectedLbl = findViewById(R.id.connectedDeviceLbl);
//            deviceConnectedLbl.setText("Conectado al dispositivo: " + ssid);
//        }
    }

    private BroadcastReceiver wifiReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();
            if (action != null && !action.isEmpty()) {
                if(action.equals(WifiManager.NETWORK_STATE_CHANGED_ACTION)) {
                    updateDeviceConnected();
                }
            }
        }
    };

    private void updateDeviceConnected() {
        TextView stateLbl = (TextView) findViewById(R.id.stateLbl);
        ImageButton stateColor = (ImageButton) findViewById(R.id.colorState);
        TextView deviceConnectedLbl = (TextView) findViewById(R.id.connectedDeviceLbl);
        ImageButton auscultateBtn = findViewById(R.id.auscultateBtn);
        TextView auscultateLbl = (TextView) findViewById(R.id.auscultateLbl);
        if (wifiManager.isWifiEnabled()) {
            WifiInfo wifiInfo = wifiManager.getConnectionInfo();
            if(wifiInfo.getNetworkId() != -1 ){
                stateLbl.setText("Conectado");
                stateColor.setBackgroundResource(android.R.drawable.presence_online);
                deviceConnectedLbl.setText("Dispositivo: " + wifiInfo.getSSID());
                auscultateBtn.setVisibility(View.VISIBLE);
                auscultateLbl.setVisibility(View.VISIBLE);
            } else {
                stateLbl.setText("Desconectado");
                stateColor.setBackgroundResource(android.R.drawable.presence_offline);
                deviceConnectedLbl.setText("");
                auscultateBtn.setVisibility(View.GONE);
                auscultateLbl.setVisibility(View.GONE);
            }
        } else {
            stateLbl.setText("Desconectado");
            stateColor.setBackgroundResource(android.R.drawable.presence_offline);
            deviceConnectedLbl.setText("");
            auscultateBtn.setVisibility(View.GONE);
            auscultateLbl.setVisibility(View.GONE);
        }
    }

    public void auscultate(View view) {
        Intent intent = new Intent(this, AscultateActivity.class);
        startActivity(intent);
    }
}
