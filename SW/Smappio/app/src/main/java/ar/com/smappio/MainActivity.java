package ar.com.smappio;

import android.Manifest;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.support.design.widget.NavigationView;
import android.support.v4.view.GravityCompat;
import android.support.v4.widget.DrawerLayout;
import android.support.v7.app.ActionBarDrawerToggle;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.ImageButton;
import android.widget.TextView;

import java.io.File;

public class MainActivity extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {

    private WifiManager wifiManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // Chequear si tiene permisos necesarios, caso contrario se cierra la aplicacion y los pide devuelta
        checkTotalPermission();

        // Crear carpeta por primera vez de smappio - ../DCIM/Smappio
        createSmappioFolder();

        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        DrawerLayout drawer = findViewById(R.id.drawer_layout);
        ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, R.string.navigation_drawer_open, R.string.navigation_drawer_close);
        drawer.addDrawerListener(toggle);
        toggle.syncState();

        NavigationView navigationView = findViewById(R.id.nav_view);
        navigationView.setNavigationItemSelectedListener(this);

        wifiManager = (WifiManager) getApplicationContext().getSystemService(Context.WIFI_SERVICE);
    }

    // Al presionar el botón fisico "volver atrás (<)", si esta abierta la sidebar, se cierra
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
    }

    @SuppressWarnings("StatementWithEmptyBody")
    @Override
    public boolean onNavigationItemSelected(MenuItem item) {
        int id = item.getItemId();

        if (id == R.id.nav_files) {
            openFileSystem();
        } else if (id == R.id.nav_help) {
            // Abrir popup de ayuda
        }

        DrawerLayout drawer = findViewById(R.id.drawer_layout);
        drawer.closeDrawer(GravityCompat.START);
        return true;
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        if (requestCode == Constant.CODE_PERMISSIONS_REQUEST_CODE) {
            boolean grantedAllPermission = true;
            for (int grantResult : grantResults) {
                if (grantResult != PackageManager.PERMISSION_GRANTED) {
                    grantedAllPermission = false;
                }
            }
            if(!grantedAllPermission) {
                AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(this);
                LayoutInflater layoutInflater = LayoutInflater.from(MainActivity.this);
                View popupAlertPermissions = layoutInflater.inflate(R.layout.popup_alert_permissions, null);
                alertDialogBuilder.setView(popupAlertPermissions);
                alertDialogBuilder.setTitle(R.string.str_alerta);
                alertDialogBuilder
                        .setCancelable(false)
                        .setPositiveButton(R.string.str_aceptar,
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

    // Metodo para chequear y solicitar permisos que necesita la aplicacion
    public void checkTotalPermission() {

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M &&
                !(checkSelfPermission(Manifest.permission.READ_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.WRITE_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.READ_PHONE_STATE) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.WAKE_LOCK) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.MODIFY_AUDIO_SETTINGS) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.RECORD_AUDIO) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.INTERNET) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.ACCESS_WIFI_STATE) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.CHANGE_WIFI_STATE) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.ACCESS_FINE_LOCATION) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.CHANGE_NETWORK_STATE) == PackageManager.PERMISSION_GRANTED
                        && checkSelfPermission(Manifest.permission.ACCESS_NETWORK_STATE) == PackageManager.PERMISSION_GRANTED)) {

            requestPermissions(new String[]{
                    Manifest.permission.READ_EXTERNAL_STORAGE,
                    Manifest.permission.WRITE_EXTERNAL_STORAGE,
                    Manifest.permission.READ_PHONE_STATE,
                    Manifest.permission.WAKE_LOCK,
                    Manifest.permission.MODIFY_AUDIO_SETTINGS,
                    Manifest.permission.RECORD_AUDIO,
                    Manifest.permission.INTERNET,
                    Manifest.permission.ACCESS_WIFI_STATE,
                    Manifest.permission.CHANGE_WIFI_STATE,
                    Manifest.permission.ACCESS_FINE_LOCATION,
                    Manifest.permission.CHANGE_NETWORK_STATE,
                    Manifest.permission.ACCESS_NETWORK_STATE
            }, Constant.CODE_PERMISSIONS_REQUEST_CODE);
        }
    }

    // Metodo para crear carpeta de smappio al iniciar la app por primera vez. "../DCIM/Smappio"
    public void createSmappioFolder() {
        File file = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM), getString(R.string.str_smappio));
        if (!file.exists()) {
            Log.d("FOLDER", "Folder doesn't exist, creating it...");
            boolean rv = file.mkdir();
            Log.d("FOLDER", "Folder creation " + ( rv ? "success" : "failed"));
        } else {
            Log.d("FOLDER", "Folder already exists.");
        }
    }

    // Metodo para abrir la FileChooserActivity
    public void openFileSystem(){
        Intent intent = new Intent(this, FileChooserActivity.class);
        startActivity(intent);
    }

    // Metodo para abrir la WifiActivity
    public void openWifiScanActivity(View view) {
        Intent intent = new Intent(this, WifiActivity.class);
        startActivity(intent);
    }

    // Metodo para abrir la AuscultateActivity
    public void auscultate(View view) {
        Intent intent = new Intent(this, AuscultateActivity.class);
        startActivity(intent);
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
        if (wifiManager.isWifiEnabled()) {
            WifiInfo wifiInfo = wifiManager.getConnectionInfo();
            if(wifiInfo.getNetworkId() != -1 && wifiInfo.getSSID().contains("Smappio")){
                updateConnectInfo(true, wifiInfo.getSSID());
            } else {
                updateConnectInfo(false, null);
            }
        } else {
            updateConnectInfo(false, null);
        }
    }

    private void updateConnectInfo(Boolean connected, String ssid) {
        TextView stateLbl = findViewById(R.id.state_lbl);
        ImageButton stateColor = findViewById(R.id.state_icon);
        TextView deviceConnectedLbl = findViewById(R.id.connected_device_lbl);
        ImageButton connectBtn = findViewById(R.id.connect_btn);
        TextView connectLbl = findViewById(R.id.connect_lbl);
        ImageButton auscultateBtn = findViewById(R.id.auscultate_btn);
        TextView auscultateLbl = findViewById(R.id.auscultate_lbl);

        if(connected) {
            stateLbl.setText(R.string.str_conectado);
            stateColor.setBackgroundResource(android.R.drawable.presence_online);
            deviceConnectedLbl.setText(getString(R.string.msg_vinculado_a, ssid));
            auscultateBtn.setVisibility(View.VISIBLE);
            auscultateLbl.setVisibility(View.VISIBLE);
            connectBtn.setVisibility(View.GONE);
            connectLbl.setVisibility(View.GONE);
        } else {
            stateLbl.setText(R.string.str_desconectado);
            stateColor.setBackgroundResource(android.R.drawable.presence_offline);
            deviceConnectedLbl.setText("");
            auscultateBtn.setVisibility(View.GONE);
            auscultateLbl.setVisibility(View.GONE);
            connectBtn.setVisibility(View.VISIBLE);
            connectLbl.setVisibility(View.VISIBLE);
        }
    }
}
