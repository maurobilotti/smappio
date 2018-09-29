package ar.com.smappio;

import android.Manifest;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.net.wifi.ScanResult;
import android.net.wifi.WifiConfiguration;
import android.net.wifi.WifiManager;
import android.os.Build;
import android.os.Bundle;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.text.method.HideReturnsTransformationMethod;
import android.text.method.PasswordTransformationMethod;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.Switch;
import android.widget.Toast;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

public class WifiActivity extends AppCompatActivity {

    private WifiManager wifiManager;
    private List<ScanResult> networkLst = new ArrayList<>();
    private ArrayList<String> ssidLst = new ArrayList<>();
    private ArrayAdapter adapter;
    private Button scanBtn;
    private Switch wifiBtn;

    @Override
    public void onCreate(Bundle savedInstanceState) {

        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_wifi);

        //Icono para volver a la activity anterior
        if (getSupportActionBar() != null){
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setDisplayShowHomeEnabled(true);
        }

        wifiManager = (WifiManager) getApplicationContext().getSystemService(Context.WIFI_SERVICE);

        scanBtn = findViewById(R.id.scanBtn);
        wifiBtn = findViewById(R.id.wifiBtn);

        adapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, ssidLst);
        ListView ssidListView = findViewById(R.id.wifiList);
        ssidListView.setOnItemClickListener(onItemClickListener);
        ssidListView.setAdapter(adapter);
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
        unregisterReceiver(wifiReceiver);
    }

    @Override
    protected void onResume() {
        super.onResume();
        registerReceiver(wifiReceiver, new IntentFilter(WifiManager.WIFI_STATE_CHANGED_ACTION));
    }

    public void updateWifi(View view) {
        ssidLst.clear();
        networkLst.clear();
        if(wifiManager.isWifiEnabled()) {
            Toast.makeText(this, "Buscando Dispositivo ...", Toast.LENGTH_SHORT).show();
            networkLst = wifiManager.getScanResults();
            for (ScanResult scanResult : networkLst) {
                ssidLst.add(scanResult.SSID);//+ " - " + scanResult.capabilities);
            }
        }
        adapter.notifyDataSetChanged();
    }

    public void wifiOnOff(View view) {
        if(!wifiBtn.isChecked()) {
            wifiManager.setWifiEnabled(false);
        } else {
            wifiManager.setWifiEnabled(true);
        }
    }

    private BroadcastReceiver wifiReceiver = new BroadcastReceiver() {

        @Override
        public void onReceive(Context context, Intent intent) {

            String action = intent.getAction();

            if (action != null && !action.isEmpty()) {
                if(action.equals(WifiManager.WIFI_STATE_CHANGED_ACTION)) {
                    if(!wifiManager.isWifiEnabled()) {
                        scanBtn.setEnabled(false);
                        wifiBtn.setChecked(false);
                    } else {
                        scanBtn.setEnabled(true);
                        wifiBtn.setChecked(true);
                    }
                    updateWifi(scanBtn);
                }
            }
        }
    };

    private AdapterView.OnItemClickListener onItemClickListener = new AdapterView.OnItemClickListener() {

        private String ssid;
        private String password;
        private String capabilities;

        @Override
        public void onItemClick(AdapterView<?> parent, View view, int position, long id) {

            ssid = (String) parent.getAdapter().getItem(position);

            Iterator<ScanResult> iterator = networkLst.iterator();
            ScanResult networkSelected = null;
            while (iterator.hasNext() && networkSelected == null) {
                ScanResult sr = iterator.next();
                if (sr.SSID.equals(ssid)) {
                    networkSelected = sr;
                }
            }

            if (networkSelected != null) {

                capabilities = networkSelected.capabilities;

                if (!capabilities.toUpperCase().contains("WEP") && !capabilities.toUpperCase().contains("WPA")) {

                    connectWiFi(ssid, null, capabilities);

                } else {

                    LayoutInflater layoutInflater = LayoutInflater.from(WifiActivity.this);
                    View popupWifiView = layoutInflater.inflate(R.layout.popup_wifi_connection, null);

                    AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(WifiActivity.this);
                    alertDialogBuilder.setView(popupWifiView);
                    alertDialogBuilder.setTitle("Conectarse a " + ssid);
                    EditText userInput = popupWifiView.findViewById(R.id.passwordEditText);

                    CheckBox checkbox = popupWifiView.findViewById(R.id.showPasswordCheckbox);
                    checkbox.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
                        @Override
                        public void onCheckedChanged(CompoundButton compoundButton, boolean isChecked) {
                            if (isChecked) {
                                userInput.setTransformationMethod(HideReturnsTransformationMethod.getInstance());
                            } else {
                                userInput.setTransformationMethod(PasswordTransformationMethod.getInstance());
                            }
                        }
                    });

                    alertDialogBuilder.setCancelable(false).setPositiveButton("Aceptar",
                            new DialogInterface.OnClickListener() {
                                public void onClick(DialogInterface dialog, int id) {
                                    // get user input from user for password
                                    password = userInput.getText().toString();
                                    //Call the connectWiFi method to get connected the network
                                    connectWiFi(ssid, password, capabilities);
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
            }
        }

        public void connectWiFi(String SSID, String password, String Security) {
            try {
                Log.d("INFO", "Item clicked, SSID " + SSID + " Security : " + Security);
                String networkSSID = SSID;
                String networkPass = password;
                WifiConfiguration conf = new WifiConfiguration();
                // Please note the quotes. String should contain ssid in quotes
                conf.SSID = "\"" + networkSSID + "\"";
                conf.status = WifiConfiguration.Status.ENABLED;
                conf.priority = 40;
                // Check if security type is WEP
                if (Security.toUpperCase().contains("WEP")) {
                    Log.v("INFO", "Configuring WEP");
                    conf.allowedKeyManagement.set(WifiConfiguration.KeyMgmt.NONE);
                    conf.allowedProtocols.set(WifiConfiguration.Protocol.RSN);
                    conf.allowedProtocols.set(WifiConfiguration.Protocol.WPA);
                    conf.allowedAuthAlgorithms.set(WifiConfiguration.AuthAlgorithm.OPEN);
                    conf.allowedAuthAlgorithms.set(WifiConfiguration.AuthAlgorithm.SHARED);
                    conf.allowedPairwiseCiphers.set(WifiConfiguration.PairwiseCipher.CCMP);
                    conf.allowedPairwiseCiphers.set(WifiConfiguration.PairwiseCipher.TKIP);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.WEP40);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.WEP104);
                    if (networkPass.matches("^[0-9a-fA-F]+$")) {
                        conf.wepKeys[0] = networkPass;
                    } else {
                        conf.wepKeys[0] = "\"".concat(networkPass).concat("\"");
                    }
                    conf.wepTxKeyIndex = 0;
                // Check if security type is WPA
                } else if (Security.toUpperCase().contains("WPA")) {
                    Log.v("INFO", "Configuring WPA");
                    conf.allowedProtocols.set(WifiConfiguration.Protocol.RSN);
                    conf.allowedProtocols.set(WifiConfiguration.Protocol.WPA);
                    conf.allowedKeyManagement.set(WifiConfiguration.KeyMgmt.WPA_PSK);
                    conf.allowedPairwiseCiphers.set(WifiConfiguration.PairwiseCipher.CCMP);
                    conf.allowedPairwiseCiphers.set(WifiConfiguration.PairwiseCipher.TKIP);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.WEP40);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.WEP104);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.CCMP);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.TKIP);
                    conf.preSharedKey = "\"" + networkPass + "\"";
                // Check if network is open network
                } else {
                    Log.v("INFO", "Configuring OPEN network");
                    conf.allowedKeyManagement.set(WifiConfiguration.KeyMgmt.NONE);
                    conf.allowedProtocols.set(WifiConfiguration.Protocol.RSN);
                    conf.allowedProtocols.set(WifiConfiguration.Protocol.WPA);
                    conf.allowedAuthAlgorithms.clear();
                    conf.allowedPairwiseCiphers.set(WifiConfiguration.PairwiseCipher.CCMP);
                    conf.allowedPairwiseCiphers.set(WifiConfiguration.PairwiseCipher.TKIP);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.WEP40);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.WEP104);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.CCMP);
                    conf.allowedGroupCiphers.set(WifiConfiguration.GroupCipher.TKIP);
                }

                //Connect to the network
                WifiManager wifiManager = (WifiManager) WifiActivity.this.getApplicationContext().getSystemService(Context.WIFI_SERVICE);
                int networkId = wifiManager.addNetwork(conf);
                Log.v("INFO", "Add result " + networkId);
                List<WifiConfiguration> list = wifiManager.getConfiguredNetworks();
                for (WifiConfiguration i : list) {
                    if (i.SSID != null && i.SSID.equals("\"" + networkSSID + "\"")) {
                        Log.v("INFO", "WifiConfiguration SSID " + i.SSID);
                        boolean isDisconnected = wifiManager.disconnect();
                        Log.v("INFO", "isDisconnected : " + isDisconnected);
                        boolean isEnabled = wifiManager.enableNetwork(i.networkId, true);
                        Log.v("INFO", "isEnabled : " + isEnabled);
                        boolean isReconnected = wifiManager.reconnect();
                        Log.v("INFO", "isReconnected : " + isReconnected);
                        break;
                    }
                }

                Toast.makeText(WifiActivity.this, "Se conectó al dispositivo", Toast.LENGTH_LONG).show();

                //Devolver datos al activity padre (Quien inicio con startActivityForResult)
//                Intent response = new Intent();
//                response.putExtra("networkId", networkId);
//                response.putExtra("ssid", ssid);
//                setResult(Constant.CODE_WIFI_CONNECTED, response);
                finish();

            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    };

}
