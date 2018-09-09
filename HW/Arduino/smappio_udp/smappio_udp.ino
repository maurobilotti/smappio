#include "libraries/SmappioSound/SmappioSound.cpp"
#include <WiFi.h>
#include <WiFiUdp.h>

WiFiServer server(80);
WiFiUDP udpClientServer;
 
const char* ssid     = "SMAPPIO_UDP_SERVER";
const char* password = "123456789"; // El pass tiene que tener mas de 8 caracteres

// CONSTANTES
#define BYTES_TO_SEND   42000   //57438 es aparentemente el max   // Tiene que ser multiplo de 3  
#define MEDIA 13700   // I2S:  13700  | PCM:  6835   // valor para nivelar a 0 la señal media
#define WIFI_CHANNEL 1  //  1  |  6  |  11

// VARIABLES
int32_t *_buffer;
uint8_t _dataToSend[BYTES_TO_SEND + 1];
SmappioSound smappioSound(MEDIA); 

void setup() { 
  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  smappioSound.begin(_buffer);
  Serial.begin(115200);

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
  Serial.println("Esperando conexión.");
  // Si el cliente inicio el handshake
  if (client) 
  {
    int bufferSeqNum = 0;      
    Serial.println("Enviando un mensaje:");
    Serial.println(client.localIP());

    while (client.connected()) 
    {
      bool response = udpSendMessage(client.localIP(), 1222,"123456789");
      if(response)
      {
        Serial.println("Mensaje enviado");  
      }
    }
    /*while (client.connected()) 
    {      
      if(bufferSeqNum == 64)
        bufferSeqNum = 0;
        
       bufferSamplesToSendWithControlBits();

      // Se agrega el numero de secuencia del bloque en el ultimo byte del mismo, con los 2 primeros bits en '00'y se envia
      _dataToSend[BYTES_TO_SEND] = bufferSeqNum & 63;
      client.write(_dataToSend, BYTES_TO_SEND + 1);
      Udp.write(_dataToSend, BYTES_TO_SEND + 1);
      bufferSeqNum++;
    } */
  } 
}


bool udpSendMessage(IPAddress ipAddr,int udpPort, String udpMsg) {
  /** WiFiUDP class for creating UDP communication */
  WiFiUDP udpClientServer;

  // Start UDP client for sending packets
  int connOK = udpClientServer.begin(udpPort);

  if (connOK == 0) {
    Serial.println("UDP could not get socket");
    return false;
  }
  int beginOK = udpClientServer.beginPacket(ipAddr, udpPort);

  if (beginOK == 0) { // Problem occured!
    udpClientServer.stop();
    Serial.println("UDP connection failed");
    return false;
  }
  int bytesSent = udpClientServer.print(udpMsg);
  if (bytesSent == udpMsg.length()) {
    // Serial.println("Sent " + String(bytesSent) + " bytes from " + udpMsg + " which had a length of " + String(udpMsg.length()) + " bytes");
    udpClientServer.endPacket();
    udpClientServer.stop();
    return true;
  } else {
    Serial.println("Failed to send " + udpMsg + ", sent " + String(bytesSent) + " of " + String(udpMsg.length()) + " bytes");
    udpClientServer.endPacket();
    udpClientServer.stop();
    return false;
  }
}
// Metodo para bufferear sonido con bits de control
void bufferSamplesToSendWithControlBits() 
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
    value = smappioSound.getSampleValue();
    
    // Se bufferean los 3 bytes del sample, con un desplazamiento y una numeracion
    // 'a': bit del primer byte. 'b': bit del segundo byte. 'c': bit del tercer byte.
    _dataToSend[i] = (value & 63) | 64;               // '64'   =  01|aaaaaa
    _dataToSend[i + 1] = ((value >> 6)  & 63) | 128;  // '128'  =  10|bbbbaa
    _dataToSend[i + 2] = ((value >> 12) & 63) | 192;  // '192'  =  11|ccbbbb
  }  
}

// Metodo para bufferear sonido sin bits de control
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
    value = smappioSound.getSampleValue();

    // Se bufferean los 3 bytes del sample
    _dataToSend[i] = value & 255;
    _dataToSend[i + 1] = (value >> 8)  & 255;
    _dataToSend[i + 2] = (value >> 16) & 255; 
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

// Metodo para enviar la siguiente secuencia valida de enteros: 0, 3, -6, 9, -12, ... 
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
