#include "libraries/SmappioSound/SmappioSound.cpp"
#include <WiFi.h>

WiFiServer server(80);
 
const char* ssid     = "smappio";
const char* password = "123456789"; // El pass tiene que tener mas de 8 caracteres

int32_t *buffer;
int32_t value = 0;
int bytesReaded = 0;
int media = 13700;  // I2S:  13700  | PCM:  6835   // valor para nivelar a 0 la señal media

long samplesCounter = 0;
long samplesLimit = 32000 * 15;
uint8_t dataToSend[42000];
int pos = 0;

SmappioSound smappioSound(media); 

void setup() { 
  // Se crea un Access Point, para poder conectarse al dispositivo
  WiFi.softAP(ssid, password);
  // Se setea la ip del dispositivo para poder comunicarse con el
  IPAddress Ip(192, 168, 1, 1);
  IPAddress NMask(255, 255, 255, 0);
  WiFi.softAPConfig(Ip, Ip, NMask);
  
  
  server.begin();
  Serial.begin(9600);
  smappioSound.begin(buffer);  
}

void loop() {
  // Se escucha a las conexiones a ver si algun cliente se quiere comunicar (iniciar el handshake)
  WiFiClient client = server.available(); 
 
  // Si el cliente inicio el handshake
  if (client) 
  {       
    while (client.connected()) 
    {      
      for(int i = 0; i < (int)sizeof(dataToSend); i += 3)
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
          //value = 30;
          dataToSend[i] = value & 255;
          dataToSend[i + 1] = (value >> 8)  & 255;
          dataToSend[i + 2] = (value >> 16) & 255;     
      }
      
      /*for(int i = 0; i < (int)sizeof(dataToSend); i++)
      {
        printf("%d", dataToSend[i]);
        //Serial.print(dataToSend[i]);
        Serial.print("/");
      }*/
      
      client.write(dataToSend, sizeof(dataToSend));
      
      /*if(pos <= 150)
      {
        strcat(dataToSend, bufferTemp);
        pos += 3;
      }
      else 
      {
        pos=0;  
        const char *p = reinterpret_cast<const char*>(dataToSend);
        webSocketServer.sendData(p);
      }*/
   } 
  } 
}