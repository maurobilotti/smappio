#include <WiFi101.h>

WiFiServer server(80);
 
const char* ssid     = "smappio";
const char* password = "123456789"; // El pass tiene que tener mas de 8 caracteres

// CONSTANTES
#define BYTES_TO_SEND   3000//1435   //57438 es aparentemente el max   // Tiene que ser multiplo de 3  
#define WIFI_CHANNEL 1  //  1  |  6  |  11

// VARIABLES
int32_t *_buffer;
uint8_t _dataToSend[BYTES_TO_SEND];

void setup() { 
  // Se crea un Access Point, para poder conectarse al dispositivo
  //WiFi.softAP(ssid, password, WIFI_CHANNEL);
  WiFi.beginAP(ssid);
  delay(300); // DEJAR ESTE DELAY, sino softAP se puede colgar.

  // Se setea la ip del dispositivo para poder comunicarse con el
  IPAddress gateway(192,168,1,1);
  IPAddress Ip(192, 168, 1, 2);
  IPAddress NMask(255, 255, 255, 0);
  //WiFi.softAPConfig(Ip, gateway, NMask);
  WiFi.config(Ip); 
  server.begin();
}

void loop() {
  // Se escucha a las conexiones a ver si algun cliente se quiere comunicar (iniciar una conexion)
  WiFiClient client = server.available(); 
 
  // Si el cliente inicio una conexion
  if (client) 
  {
    while (client.connected()) 
    {      
      bufferSamplesToSend();

      client.write(_dataToSend, BYTES_TO_SEND);
    } 
  } 
}

// Metodo para bufferear "sonido" sin bits de control
void bufferSamplesToSend() 
{
  int32_t value = 0;

  for(int i = 0; i < BYTES_TO_SEND; i += 3)
  {
    // Se mockea la lectura de un sample del microfono
    {
        //value = smappioSound.getSampleValue();
        delayMicroseconds(31);
        value = i;
    }

    // Se bufferean los 3 bytes del sample
    _dataToSend[i] = value & 255;
    _dataToSend[i + 1] = (value >> 8)  & 255;
    _dataToSend[i + 2] = (value >> 16) & 255; 
  }  
}