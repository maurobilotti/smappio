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

#define SMAPPIO_CODE_ERROR -1
#define SMAPPIO_CODE_SEND_DATA 1
#define SMAPPIO_CODE_TEST 2
#define SMAPPIO_CODE_LOGS 3

#define LOGGER_SIZE 40

enum logAudit {
  ON, // 0
  OFF, // 1
  CONNECTED, // 2
  DISCONNECTED, // 3
  TRANSMITTED, // 4
  TESTED // 5
};

//ESTRUCTURAS
struct Log
{  
 int idLog;//Autonumerico
 logAudit codeLog; // Código de acción auditada
};
  
// VARIABLES
int32_t *_buffer;
uint8_t _dataToSend[BYTES_TO_SEND + 1];
SmappioSound smappioSound(MEDIA); 

//AUDITORIA
RTC_DATA_ATTR Log logger[LOGGER_SIZE];
RTC_DATA_ATTR int indexLog = 0;
RTC_DATA_ATTR bool loggerFull = false;
RTC_DATA_ATTR int idLog = 1;

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
  
  addLogAudit(ON);
}

void loop() {
  deepSleepIfButtonPressed();
  
  // Se escucha a las conexiones a ver si algun cliente se quiere comunicar (iniciar el handshake)
  WiFiClient client = server.available(); 
 
  // Si el cliente inicio el handshake
  if (client) 
  {
    client.setNoDelay(TCP_NO_DELAY);  
    addLogAudit(CONNECTED);
    
    while (client.connected()) 
    {
      deepSleepIfButtonPressed();
      int code = readCode(client);    // send = 1  |  test = 2
      clearBuffer(client);

      if(code != SMAPPIO_CODE_ERROR)
        digitalWrite(CONNECTED_LED, HIGH);
      
      if (code == SMAPPIO_CODE_SEND_DATA)
      {
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
          addLogAudit(TRANSMITTED);
        }
      }
      else if (code == SMAPPIO_CODE_TEST)
      {
        while(client.connected())
        {
          deepSleepIfButtonPressed();
          bufferAlternateMinMaxValue();
          client.write(_dataToSend, BYTES_TO_SEND);
          sleep(1); // Para no saturar la transferencia 
          addLogAudit(TESTED);
        }
      }
      else if(code == SMAPPIO_CODE_LOGS)
      {
        bool sended = false;
        while(client.connected())
        {
          deepSleepIfButtonPressed();
          if(!sended)
          {
            sended = true;
            int bytesSizeToSend = bufferLogsToSend();
            client.write(_dataToSend, bytesSizeToSend);
          }
          else //Se rellena con 0s para que envie el paquete tcp
          {
            for (int i = 0; i < BYTES_TO_SEND; i++)
            {
              _dataToSend[i] = 0;
            }
            client.write(_dataToSend, BYTES_TO_SEND);
          }
          sleep(1); // Para no saturar la transferencia 
        }
      }
    }
  }
  else
  {
    digitalWrite(CONNECTED_LED, LOW);
    addLogAudit(DISCONNECTED);
  }
}

void addLogAudit(logAudit codeLog)
{
  int previousIndex = -1;
  if(indexLog == 0)
  { 
    if(loggerFull)
      previousIndex = LOGGER_SIZE - 1;
  }
  else
  {
    previousIndex = indexLog - 1;
  }  
  
  if((previousIndex == -1 || logger[previousIndex].codeLog != codeLog) // si es el primero O si el anterior es distinto
    && (codeLog != DISCONNECTED || (codeLog == DISCONNECTED && logger[previousIndex].codeLog != ON))) // si el codigo NO es disconnected o si al ser disconnected, el anterior no es ON
  {
    logger[indexLog].idLog = idLog;
    logger[indexLog].codeLog = codeLog;
  
    idLog++;
    if(indexLog == LOGGER_SIZE - 1)
      {
        indexLog = 0;
        loggerFull = true;
      }
    else
      indexLog++;
  }
}

// Bufferea los logs a enviar. Retorna la cantidad de bytes a enviar
int bufferLogsToSend() 
{
  int baseIndex = 0;
  if(loggerFull)
  {
    for(int i = indexLog; i < LOGGER_SIZE; i++)
    {
      // ID de log
      _dataToSend[baseIndex] = (logger[i].idLog & 255);
      _dataToSend[baseIndex + 1] = (logger[i].idLog >> 6) & 255;
      _dataToSend[baseIndex + 2] = (logger[i].idLog >> 12) & 255;
      _dataToSend[baseIndex + 3] = (logger[i].idLog >> 24) & 255;

      // Codigo de log
      _dataToSend[baseIndex + 4] = (logger[i].codeLog & 255);
      _dataToSend[baseIndex + 5] = (logger[i].codeLog >> 6) & 255;
      _dataToSend[baseIndex + 6] = (logger[i].codeLog >> 12) & 255;
      _dataToSend[baseIndex + 7] = (logger[i].codeLog >> 24) & 255;
      
      baseIndex += 8;
    }
  }
  
  // Siempre pasa por este for
  for(int i = 0; i < indexLog; i++)
  {
    // ID de log
    _dataToSend[baseIndex] = (logger[i].idLog & 255);
    _dataToSend[baseIndex + 1] = (logger[i].idLog >> 6) & 255;
    _dataToSend[baseIndex + 2] = (logger[i].idLog >> 12) & 255;
    _dataToSend[baseIndex + 3] = (logger[i].idLog >> 24) & 255;

    // Codigo de log
    _dataToSend[baseIndex + 4] = (logger[i].codeLog & 255);
    _dataToSend[baseIndex + 5] = (logger[i].codeLog >> 6) & 255;
    _dataToSend[baseIndex + 6] = (logger[i].codeLog >> 12) & 255;
    _dataToSend[baseIndex + 7] = (logger[i].codeLog >> 24) & 255;
    
    baseIndex += 8;
  }

  return baseIndex; // por la ultima pasada
}

int readCode(WiFiClient client)
{
  char firstChar;
  if (client.available())
  {
    firstChar = readChar(client);
    if(firstChar == 's')
    {
      if(readChar(client) == 'e')
        if(readChar(client) == 'n')
          if(readChar(client) == 'd')
            return SMAPPIO_CODE_SEND_DATA;
    }
    else if(firstChar == 't')
    {
      if(readChar(client) == 'e')
        if(readChar(client) == 's')
          if(readChar(client) == 't')
            return SMAPPIO_CODE_TEST;
    }
    else if(firstChar == 'l')
    {
      if(readChar(client) == 'o')
        if(readChar(client) == 'g')
          if(readChar(client) == 's')
            return SMAPPIO_CODE_LOGS;
    }
  }
  return -1; // error
}

char readChar(WiFiClient client)
{
  while(!client.available())
  {
    // do nothing
  }
  return client.read();
}

void clearBuffer(WiFiClient client)
{
  //esto es un workaround que funciono en algun momento, lo dejo por las dudas
  //client.write(63); // envia el byte 00111111
}

void deepSleepIfButtonPressed() {
  if(digitalRead(POWER_BUTTON) == 0)
  {
    digitalWrite(ON_LED, LOW);
    digitalWrite(CONNECTED_LED, LOW);
    addLogAudit(OFF);
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

void bufferAlternateMinMaxValue()
{
  int32_t value = 0;

  for(int i = 0; i < BYTES_TO_SEND; i += 3)
  {
    value = MAX_SAMPLE_VALUE;
    if(i % 2 == 0)
      value = MIN_SAMPLE_VALUE;

    // Se bufferean los 3 bytes del sample
    // OJO Con el flag CONTROL_ALGHORITM_ENABLED
    addSampleToBuffer(i, value);
  } 
}

/////////////////////////////      FIN REGION DE FUNCIONES DE PRUEBA DE TRANSMISION      /////////////////////////////
