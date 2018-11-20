#include <WiFi.h>
#include <esp_wifi.h>
#include <esp_sleep.h>
#include"driver/rtc_io.h"
#include "libraries/SmappioSound/SmappioSound.cpp"

WiFiServer server(80);
 
const char* ssid     = "Smappio";
const char* password = "123456789"; // El pass tiene que tener mas de 8 caracteres

// CONSTANTES
#define MAX_SAMPLE_VALUE 131071
#define MIN_SAMPLE_VALUE -131072

#define TCP_NO_DELAY true 
#define BYTES_TO_SEND  345                  // 57438 es aparentemente el max   // Tiene que ser multiplo de 3  
#define MEDIA 13700                         // I2S:  13700  | PCM:  6835   // valor para nivelar a 0 la señal media
#define WIFI_CHANNEL 1                      // 1  |  6  |  11
#define CONTROL_ALGHORITM_ENABLED true     // Habilitar para enviar los bits de control

#define ON_LED 26
#define CONNECTED_LED 25
#define LOW_BATTERY_LED 14
#define POWER_BUTTON GPIO_NUM_4

// VARIABLES
int32_t *_buffer;
uint8_t _dataToSend[BYTES_TO_SEND + 1];
SmappioSound smappioSound(MEDIA); 

void setup() { 
  pinMode(ON_LED, OUTPUT);
  pinMode(CONNECTED_LED, OUTPUT);
  pinMode(LOW_BATTERY_LED, OUTPUT);
  pinMode(POWER_BUTTON, INPUT_PULLUP); // Boton on/off
  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  digitalWrite(ON_LED, HIGH);
  smappioSound.begin(_buffer);

  // Se crea un Access Point, para poder conectarse al dispositivo
  WiFi.softAP(ssid, password, WIFI_CHANNEL);
  delay(300); // DEJAR ESTE DELAY, sino softAP se puede colgar.

  // Se setea la ip del dispositivo para poder comunicarse con el
  IPAddress gateway(0,0,0,0);
  IPAddress Ip(192, 168, 1, 2);
  IPAddress NMask(255, 255, 255, 0);
  WiFi.softAPConfig(Ip, gateway, NMask);
  server.begin();
  server.setNoDelay(TCP_NO_DELAY);

  rtc_gpio_pullup_en(POWER_BUTTON);
  esp_sleep_enable_ext0_wakeup(POWER_BUTTON, 0); //1 = High, 0 = Low
}

void loop() {
  deepSleepIfButtonPressed();
  
  // Se escucha a las conexiones a ver si algun cliente se quiere comunicar (iniciar el handshake)
  WiFiClient client = server.available(); 
 
  // Si el cliente inicio el handshake
  if (client) 
  {
    digitalWrite(CONNECTED_LED, HIGH);
    client.setNoDelay(TCP_NO_DELAY);  
    int bufferSeqNum = 0;  
    while (client.connected()) 
    {
      deepSleepIfButtonPressed();
            
      if(bufferSeqNum == 64)
        bufferSeqNum = 0;
        
      // Se agrega el numero de secuencia del bloque en el ultimo byte del mismo, con los 2 primeros bits en '00'y se envia
      _dataToSend[BYTES_TO_SEND] = bufferSeqNum & 63;

      bufferSamplesToSend();

      client.write(_dataToSend, BYTES_TO_SEND); // Hacer BYTES_TO_SEND + 1 para enviar el bufferSeqNum

      bufferSeqNum++;
    } 
  }
  else
  {
    digitalWrite(CONNECTED_LED, LOW); 
  }
}

void deepSleepIfButtonPressed() {
  if(digitalRead(POWER_BUTTON) == 0)
  {
    digitalWrite(ON_LED, LOW);
    digitalWrite(CONNECTED_LED, LOW);
    delay(1000);
    esp_wifi_stop();
    esp_deep_sleep_start();
  }
}

// Metodo para bufferear sonido sin bits de control
void bufferSamplesToSend() 
{
  int32_t value = 0;
  int bytesReaded = 0;

  for(int i = 0; i < BYTES_TO_SEND; i += 3)
  {
    // Se obtiene le valor de un sample. Al cambiar el nro del parámetro se modifica el sample rate.
    value = getOneSampleValueOfN(7); 

    // Se bufferean los 3 bytes del sample
    // OJO Con el flag CONTROL_ALGHORITM_ENABLED
    addSampleToBuffer(i, value);
  }  
}

// Funcion en la que se hacen "n" lecturas de samples del microfono, "n - 1" se descartan.
// Se retorna el valor del sample nro "n"
int32_t getOneSampleValueOfN(int n)
{
  int bytesReaded = 0;
  for(int j = 1; j <= n; j++)
  {
    bytesReaded = smappioSound.read();
    while(bytesReaded == 0)
    {     
        //Este bucle es necesario para no reenviar una muestra mas de una vez
        bytesReaded = smappioSound.read();
    }  
  }
  return smappioSound.getSampleValue();
}

void addSampleToBuffer(int index, int value)
{
  int inRangeValue = value;

  if(inRangeValue > MAX_SAMPLE_VALUE)
    inRangeValue = MAX_SAMPLE_VALUE;
  else if(inRangeValue < MIN_SAMPLE_VALUE)
    inRangeValue = MIN_SAMPLE_VALUE;

  if(CONTROL_ALGHORITM_ENABLED)
  {
    // Se bufferean los 3 bytes del sample, con un desplazamiento y una numeracion
    // 'a': bit del primer byte. 'b': bit del segundo byte. 'c': bit del tercer byte.
    _dataToSend[index] = (inRangeValue & 63) | 64;               // '64'   =  01|aaaaaa
    _dataToSend[index + 1] = ((inRangeValue >> 6)  & 63) | 128;  // '128'  =  10|bbbbaa
    _dataToSend[index + 2] = ((inRangeValue >> 12) & 63) | 192;  // '192'  =  11|ccbbbb
  }
  else
  {
    _dataToSend[index] = inRangeValue & 255;
    _dataToSend[index + 1] = (inRangeValue >> 8)  & 255;
    _dataToSend[index + 2] = (inRangeValue >> 16) & 255; 
  }
}


/////////////////////////////      REGION DE FUNCIONES DE PRUEBA DE TRANSMISION      /////////////////////////////

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

    // Se bufferean los 3 bytes del sample
    // OJO Con el flag CONTROL_ALGHORITM_ENABLED
    addSampleToBuffer(i, value);
  }  
}

void bufferAlternateOneAndMinusOne()
{
  int32_t value = 0;

  for(int i = 0; i < BYTES_TO_SEND; i += 3)
  {
    value = 1;
    if(i % 2 == 0)
      value = -1;

    // Se bufferean los 3 bytes del sample
    // OJO Con el flag CONTROL_ALGHORITM_ENABLED
    addSampleToBuffer(i, value);
  }  
}

/////////////////////////////      FIN REGION DE FUNCIONES DE PRUEBA DE TRANSMISION      /////////////////////////////
