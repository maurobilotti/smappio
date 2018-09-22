package ar.com.smappio;

import android.app.ProgressDialog;
import android.os.AsyncTask;
import android.support.design.widget.FloatingActionButton;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.TextView;

import java.io.IOException;
import java.io.InputStream;
import java.io.PrintStream;
import java.net.InetAddress;
import java.net.Socket;
import java.net.UnknownHostException;
import java.nio.charset.Charset;

public class AscultateActivity extends AppCompatActivity {
    
    private static final String ADDRESS = "192.168.1.2";
    private static final int PORT = 80;
    private static final String HANDSHAKE = "AUSCULTAR";

    FloatingActionButton streamButton;
    TextView resultView;
    private boolean flag = false;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_stream);

        //Flecha de la toolbar para volver al activity anterior
        if (getSupportActionBar() != null){
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setDisplayShowHomeEnabled(true);
        }

        resultView = findViewById(R.id.resultView);
        streamButton = findViewById(R.id.stream);
        streamButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                //TODO: Esto es temporal, para el floating button
                if(flag == true) {
                    streamButton.setImageResource(R.drawable.ic_heart_red_24dp);
                    flag = false;
                } else {
                    streamButton.setImageResource(R.drawable.ic_stop_black_24dp);
                    flag = true;
                }
                TCPClientAsyncTask tcpClientAsyncTask = new TCPClientAsyncTask();
                tcpClientAsyncTask.execute(HANDSHAKE);
            }
        });
    }

    class TCPClientAsyncTask extends AsyncTask<String,Void,String>{

        ProgressDialog progressDialog;

        @Override
        protected void onPreExecute() {
            super.onPreExecute();
            progressDialog = new ProgressDialog(AscultateActivity.this);
            progressDialog.setCanceledOnTouchOutside(false);
            progressDialog.setTitle("Conectando al dispositivo");
            progressDialog.setMessage("Por favor, esperar ...");
            progressDialog.show();
        }

        @Override
        protected String doInBackground(String... values){
            try {
                //Se conecta al servidor
                InetAddress serverAddr = InetAddress.getByName(ADDRESS);
                Log.i("I/TCP Client", "Connecting...");
                Socket socket = new Socket(serverAddr, PORT);
                Log.i("I/TCP Client", "Connected to server");

                //envia peticion de cliente
                Log.i("I/TCP Client", "Send data to server");
                PrintStream output = new PrintStream(socket.getOutputStream());
                String request = values[0];
                output.println(request);

                //recibe respuesta del servidor y formatea a String
                Log.i("I/TCP Client", "Received data to server");
                InputStream stream = socket.getInputStream();
                byte[] lenBytes = new byte[1024];
                stream.read(lenBytes,0,1024);
                String received = new String(lenBytes, Charset.forName("ASCII")).trim();
                Log.i("I/TCP Client", "Received " + received);
                Log.i("I/TCP Client", "");

                //cierra conexion
                socket.close();
                return received;
            }catch (UnknownHostException ex) {
                Log.e("E/TCP Client", "" + ex.getMessage());
                return ex.getMessage();
            } catch (IOException ex) {
                Log.e("E/TCP Client", "" + ex.getMessage());
                return ex.getMessage();
            }
        }

        @Override
        protected void onPostExecute(String value){
            progressDialog.dismiss();
            resultView.setText(value);
        }
    }

}