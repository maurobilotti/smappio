#include "libraries/SmappioSound/SmappioSound.cpp"
#include <WiFi.h>

WiFiServer server(80);
 
const char* ssid     = "smappio";
const char* password = "123456789"; // El pass tiene que tener mas de 8 caracteres

// CONSTANTES
#define SAMPLES_TO_SEND 42000
#define MEDIA 13700   // I2S:  13700  | PCM:  6835   // valor para nivelar a 0 la señal media

// VARIABLES
int32_t *_buffer;
uint8_t _dataToSend[SAMPLES_TO_SEND];
SmappioSound smappioSound(MEDIA); 

void setup() { 
  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  smappioSound.begin(_buffer);

  // Se crea un Access Point, para poder conectarse al dispositivo
  WiFi.softAP(ssid, password);
  // Se setea la ip del dispositivo para poder comunicarse con el
  IPAddress Ip(192, 168, 1, 1);
  IPAddress NMask(255, 255, 255, 0);
  WiFi.softAPConfig(Ip, Ip, NMask);
  
  server.begin();
}

void loop() {
  // Se escucha a las conexiones a ver si algun cliente se quiere comunicar (iniciar el handshake)
  WiFiClient client = server.available(); 
 
  // Si el cliente inicio el handshake
  if (client) 
  {       
    while (client.connected()) 
    {      
      bufferSamplesToSend();
    
      client.write(_dataToSend, SAMPLES_TO_SEND);
   } 
  } 
}

void bufferSamplesToSend() 
{
  int32_t value = 0;
  int bytesReaded = 0;

  for(int i = 0; i < SAMPLES_TO_SEND; i += 3)
  {
    // Se hace la lectura de los samples del microfono
    bytesReaded = smappioSound.read();
    while(bytesReaded == 0)
    {     
        //Este bucle es necesario para no reenviar una muestra mas de una vez
        bytesReaded = smappioSound.read();
    }   
    // Se lee un sample
    value = smappioSound.getSampleValue();
    _dataToSend[i] = value & 255;
    _dataToSend[i + 1] = (value >> 8)  & 255;
    _dataToSend[i + 2] = (value >> 16) & 255; 
  }  
}