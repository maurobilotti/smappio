package ar.com.smappio;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.wifi.ScanResult;
import android.net.wifi.WifiConfiguration;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.Switch;
import android.widget.TextView;
import android.widget.Toast;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

public class WifiActivity extends AppCompatActivity {

    private WifiManager wifiManager;
    private ListView listView;
    private List<ScanResult> results;
    private ArrayList<String> arrayList = new ArrayList<>();
    private ArrayAdapter adapter;
    private Button scanBtn;
    private Switch wifiBtn;

    @Override
    public void onCreate(Bundle savedInstanceState) {

        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_wifi);

        scanBtn = (Button) findViewById(R.id.scanBtn);
        wifiBtn = (Switch) findViewById(R.id.wifiBtn);

        wifiManager = (WifiManager) getApplicationContext().getSystemService(Context.WIFI_SERVICE);

        if(!wifiManager.isWifiEnabled()) {
            Toast.makeText(this, "Activar la tecnología WiFi", Toast.LENGTH_LONG).show();
            wifiBtn.setChecked(false);
            scanBtn.setEnabled(false);
        } else {
            wifiBtn.setChecked(true);
            scanBtn.setEnabled(true);
            scanWifi(null);
        }

        adapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, arrayList);

        listView = findViewById(R.id.wifiList);
        listView.setAdapter(adapter);
        listView.setOnItemClickListener(onItemClickListener);

    }

//    @Override
//    protected void onPause() {
//        unregisterReceiver(wifiReceiver);
//        super.onPause();
//    }
//
//    @Override
//    protected void onResume() {
//        registerReceiver(wifiReceiver, new IntentFilter(WifiManager.SCAN_RESULTS_AVAILABLE_ACTION));
//        super.onResume();
//    }

    public void wifiOnOff(View view) {
        if(wifiBtn.isChecked()) {
            wifiManager.setWifiEnabled(true);
            scanBtn.setEnabled(true);
        } else {
            wifiManager.setWifiEnabled(false);
            scanBtn.setEnabled(false);
            arrayList.clear();
            adapter.notifyDataSetChanged();
        }
    }

    public void scanWifi(View view) {
        Toast.makeText(this, "Buscando Dispositivo ...", Toast.LENGTH_SHORT).show();
        registerReceiver(wifiReceiver, new IntentFilter(WifiManager.SCAN_RESULTS_AVAILABLE_ACTION));
        wifiManager.startScan();
    }

    private BroadcastReceiver wifiReceiver = new BroadcastReceiver() {

        @Override
        public void onReceive(Context context, Intent intent) {

            String action = intent.getAction();

            if (action != null && !action.isEmpty()) {
                if(action.equals(WifiManager.SCAN_RESULTS_AVAILABLE_ACTION)) {
                    results = wifiManager.getScanResults();
                    arrayList.clear();
                    for (ScanResult scanResult : results) {
                        arrayList.add(scanResult.SSID);//+ " - " + scanResult.capabilities);
                    }
                    adapter.notifyDataSetChanged();
                    //Se quita el receiver para que no consuma bateria, sino queda funcionando en segundo plano y escuchando los cambios
                    unregisterReceiver(this);
                }
            }
        }
    };

    private AdapterView.OnItemClickListener onItemClickListener = new AdapterView.OnItemClickListener() {

        private String password;

        @Override
        public void onItemClick(AdapterView<?> parent, View view, int position, long id) {

            String ssidSelected = (String) parent.getAdapter().getItem(position);

            Iterator<ScanResult> iterator = results.iterator();
            ScanResult networkSelected = null;
            while(iterator.hasNext() && networkSelected == null) {
                ScanResult sr = iterator.next();
                if(sr.SSID.equals(ssidSelected)) {
                    networkSelected = sr;
                }
            }

//            LayoutInflater li = LayoutInflater.from(WifiActivity.this);
//            View popupWifiConnection = li.inflate(R.layout.popup_wifi_connection, null);
//
//            AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(WifiActivity.this);
//            alertDialogBuilder.setView(popupWifiConnection);
//
//            EditText userInput = (EditText) popupWifiConnection.findViewById(R.id.editTextPassword);
//
//            alertDialogBuilder.setCancelable(false)
//                .setPositiveButton("Aceptar",
//                    new DialogInterface.OnClickListener() {
//                        public void onClick(DialogInterface dialog,int id) {
//                            password = userInput.getText().toString();
//                        }
//                })
//                .setNegativeButton("Cancelar",
//                    new DialogInterface.OnClickListener() {
//                        public void onClick(DialogInterface dialog,int id) {
//                            dialog.cancel();
//                        }
//                });
//
//            AlertDialog alertDialog = alertDialogBuilder.create();
//            alertDialog.show();

            LayoutInflater li = LayoutInflater.from(WifiActivity.this);
            View promptsView = li.inflate(R.layout.popup_wifi_connection, null);
            AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(WifiActivity.this);
            alertDialogBuilder.setView(promptsView);
            final EditText userInput = (EditText) promptsView.findViewById(R.id.editTextPassword);
            TextView ssidText = (TextView) promptsView.findViewById(R.id.textViewSSID);
            ssidText.setText("Conectado a " + ssidSelected);
            TextView security = (TextView) promptsView.findViewById(R.id.textViewSecurity);
            security.setText("La seguridad de la red es:\n" + networkSelected.capabilities);
            alertDialogBuilder.setCancelable(false).setPositiveButton("Aceptar",
                    new DialogInterface.OnClickListener() {
                        public void onClick(DialogInterface dialog, int id) {
                            // get user input from user for password
                            password = userInput.getText().toString();
                            //Call the connectWiFi method to get connected the network
//                            connectWiFi(String.valueOf(d.getName()), password, d.capabilities);
                        }
                    }).setNegativeButton("Cancelar",
                    new DialogInterface.OnClickListener() {
                        public void onClick(DialogInterface dialog, int id) {
                            dialog.cancel();
                        }
                    });
            AlertDialog alertDialog = alertDialogBuilder.create();
            alertDialog.show();
        }

        private void connectToWifi(String networkSSID, String networkPassword) {
            if (networkSSID != null && !networkSSID.isEmpty() || networkPassword != null && !networkPassword.isEmpty()) {
                connectToWifi(networkSSID, networkPassword);

                WifiConfiguration wifiConfiguration = new WifiConfiguration();
                wifiConfiguration.SSID = String.format("\"%s\"", networkSSID);
                wifiConfiguration.preSharedKey = String.format("\"%s\"", networkPassword);

                int netId = wifiManager.addNetwork(wifiConfiguration);
                wifiManager.disconnect();
                wifiManager.enableNetwork(netId,true);
                wifiManager.reconnect();
            }
        }
    };

}
