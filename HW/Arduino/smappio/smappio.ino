#include "libraries/SmappioSound/SmappioSound.cpp"
#include "BluetoothSerial.h"

#define SAMPLES_TO_SEND 4

int32_t *buffer;
BluetoothSerial SerialBT; 
int32_t value = 0;
int bytesReaded = 0;
int media = 13700;  // I2S:  13700  | PCM:  6835   // valor para nivelar a 0 la señal media
int32_t samplesToSend[SAMPLES_TO_SEND];
uint8_t dataToSend[3 * 10];
// Vriables utilizadas para el conteo y estadisticas
long time1;
long time2;
long samplesCounter = 0;
long samplesLimit = 32000 * 15;

SmappioSound smappioSound(media); 


#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif


void setup() {
  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  Serial.begin(2000000);  //   115200   |   900000   |   2000000
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }
  SerialBT.begin("smappio"); //Bluetooth device name

  smappioSound.begin(buffer);
  time1 = millis();
}

void loop() {
    if(samplesCounter < samplesLimit)
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
          dataToSend[i] = value & 255;
          dataToSend[i + 1] = (value >> 8)  & 255;
          dataToSend[i + 2] = (value >> 16) & 255;     
      }

      Serial.write(dataToSend, sizeof(dataToSend));     
      
      // Serial.write((byte *)&samplesToSend, sizeof(int32_t) * SAMPLES_TO_SEND);
    }
    else {
      time2 = millis();
      Serial.print("\nTiempo transcurrido: ");
      Serial.println((time2 - time1));
      while(1);
    }
}


