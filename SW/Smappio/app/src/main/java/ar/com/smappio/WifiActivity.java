package ar.com.smappio;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.wifi.ScanResult;
import android.net.wifi.WifiConfiguration;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.Switch;
import android.widget.TextView;
import android.widget.Toast;

import java.util.ArrayList;
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
        }

        adapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, arrayList);

        listView = findViewById(R.id.wifiList);
        listView.setAdapter(adapter);
        listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                TextView tv = (TextView) view;

                connectToWifi((String) tv.getText(), "asd");
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
        });

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
                        adapter.notifyDataSetChanged();
                    }
                    //Se quita el receiver para que no consuma bateria, sino queda funcionando en segundo plano y escuchando los cambios
                    unregisterReceiver(this);
                }
            }
        }
    };

}
