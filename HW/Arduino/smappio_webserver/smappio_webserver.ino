#include "libraries/SmappioSound/SmappioSound.cpp"
#include <WiFi.h>

WiFiServer server(80);
 
const char* ssid     = "smappio_server";
const char* password = "123456789"; // El pass tiene que tener mas de 8 caracteres

// CONSTANTES
#define BYTES_TO_SEND   42000   //57438 es aparentemente el max   // Tiene que ser multiplo de 3  
#define MEDIA 13700   // I2S:  13700  | PCM:  6835   // valor para nivelar a 0 la señal media
#define AMPLITUDE_MULTIPLIER 50 // Multiplicador de la amplitud de cada sample
#define WIFI_CHANNEL 1  //  1  |  6  |  11

// VARIABLES
int32_t *_buffer;
uint8_t _dataToSend[BYTES_TO_SEND];
SmappioSound smappioSound(MEDIA); 

void setup() { 
  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  smappioSound.begin(_buffer);

  // Se crea un Access Point, para poder conectarse al dispositivo
  WiFi.softAP(ssid, password, WIFI_CHANNEL);
  delay(300); // DEJAR ESTE DELAY, sino softAP se puede colgar.
  // Se setea la ip del dispositivo para poder comunicarse con el
  IPAddress gateway(192,168,1,1);
  IPAddress Ip(192, 168, 1, 2);
  IPAddress NMask(255, 255, 255, 0);
  WiFi.softAPConfig(Ip, gateway, NMask);
  server.begin();
  server.setNoDelay(false);
}

void loop() {
  // Se escucha a las conexiones a ver si algun cliente se quiere comunicar (iniciar el handshake)
  WiFiClient client = server.available(); 
 
  // Si el cliente inicio el handshake
  if (client) 
  {
    client.setNoDelay(false);  
    while (client.connected()) 
    {      
      bufferSamplesToSend();
      client.write(_dataToSend, BYTES_TO_SEND);
    } 
  } 
}

void bufferSamplesToSend() 
{
  int32_t value = 0;
  int bytesReaded = 0;

  for(int i = 0; i < BYTES_TO_SEND; i += 3)
  {
    // Se hace la lectura de los samples del microfono
    bytesReaded = smappioSound.read();
    while(bytesReaded == 0)
    {     
        //Este bucle es necesario para no reenviar una muestra mas de una vez
        bytesReaded = smappioSound.read();
    }   
    // Se lee un sample
    value = smappioSound.getSampleValue() * AMPLITUDE_MULTIPLIER;

    // Se bufferean los 3 bytes del sample, con un desplazamiento y una numeracion
    _dataToSend[i] = (value & 63) | 64;               // '64'   =  01|111111
    _dataToSend[i + 1] = ((value >> 6)  & 63) | 128;  // '128'  =  10|111111
    _dataToSend[i + 2] = ((value >> 12) & 63) | 192;  // '192'  =  11|111111
  }  
}

// Metodo para enviar la siguiente secuencia invalida:
// Bits de contol: [{1,2,3}, {1,2,2}, {3,1,2}, ..., {3,1,2}]
// Bytes de datos: [{1,2,3}, {2,2,3}, {2,2,3}, {3,2,3}, ...{1+n, 2,3}]
void bufferInvalidSecuenceTest() 
{
  int32_t value = 0;

  for(int i = 0; i < BYTES_TO_SEND; i += 3)
  {
    value = 197121 + (i / 3);
    if(i == 3)
    {
      _dataToSend[i] = (value & 63) | 64;
      _dataToSend[i + 1] = ((value >> 6)  & 63) | 128;
      i-=1;
    }
    else if (i == 5) 
    {
      _dataToSend[i] = (value & 63) | 128;
      _dataToSend[i + 1] = ((value >> 6)  & 63) | 192;
      i-=1;
    }
    else
    {
      _dataToSend[i] = (value & 63) | 64;
      _dataToSend[i + 1] = ((value >> 6)  & 63) | 128;
      _dataToSend[i + 2] = ((value >> 12) & 63) | 192;
    }
  }  
}

void bufferAlternateSignTest()
{
  int32_t value = 0;

  for(int i = 0; i < BYTES_TO_SEND; i += 3)
  {
    value = i;
    if(i % 6 == 0)
      value = value * (-1);

    _dataToSend[i] = (value & 63) | 64;
    _dataToSend[i + 1] = ((value >> 6)  & 63) | 128;
    _dataToSend[i + 2] = ((value >> 12) & 63) | 192;
  }  
}