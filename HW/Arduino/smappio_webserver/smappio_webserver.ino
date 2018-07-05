#include "libraries/SmappioSound/SmappioSound.cpp"
#include <WiFi.h>
#include <WebSocketServer.h>

WiFiServer server(80);
WebSocketServer webSocketServer;
 
const char *ssid = "Smappio";
const char *password = "123456789";

#define SAMPLES_TO_SEND 4

int32_t *buffer;
int32_t value = 0;
int bytesReaded = 0;
int media = 13700;  // I2S:  13700  | PCM:  6835   // valor para nivelar a 0 la señal media
int32_t samplesToSend[SAMPLES_TO_SEND];

long samplesCounter = 0;
long samplesLimit = 32000 * 15;
int32_t dataToSend[32];
int pos = 0;

SmappioSound smappioSound(media); 

void setup() { 
  
  WiFi.softAP(ssid, password); 
  server.begin();
  smappioSound.begin(buffer);  
}

void loop() {
  WiFiClient client = server.available();
 
  if (client.connected() && webSocketServer.handshake(client)) {         
    while (client.connected()) {
      //mover logica del microfono acá.
      bytesReaded = smappioSound.read();
      while(bytesReaded == 0)
      {     
          //Este bucle es necesario para no reenviar una muestra mas de una vez
          bytesReaded = smappioSound.read();
      }   

      
      value = smappioSound.getSampleValue();      

      dataToSend[pos] = value;
      pos++;

      if(pos == 32)
      {
        pos = 0;
        const char *p = reinterpret_cast<const char*>(dataToSend);
        webSocketServer.sendData(p);
      }
      /*byte bufferToSend[3];
      bufferToSend[0] = value & 255;
      bufferToSend[1] = (value >> 8)  & 255;
      bufferToSend[2] = (value >> 16) & 255;*/
      //const char *p = reinterpret_cast<const char*>(bufferToSend);
      
      
      //webSocketServer.sendData(p);
      
   } 
  } 
}
