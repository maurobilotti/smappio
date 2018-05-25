#include <iostream>
#include <sstream>
#include "libraries/SmappioSound/SmappioSound.cpp"
#include "BluetoothSerial.h"

int *buffer;
int framesRead = 0;
BluetoothSerial serialBT; 
int value = 0;

SmappioSound smappioSound(6835); // valor al azar harcodeado para nivelar a 0 la señal media


#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif


void setup() {
  // put your setup code here, to run once:

  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  serialBT.begin("smappio_PCM");
  Serial.begin(115200);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  smappioSound.begin(buffer);
}
long accumulator = 0;
int counter = 1;
int limit = 20000;
int avg = 0;
void loop() {
  // put your main code here, to run repeatedly:
  
  framesRead = smappioSound.read() / 32;
  
  for(int index = 0; index <= framesRead; index++)
  {
      value = smappioSound.getSampleValue(index);
      if(index % 2 == 0)
      {
          /*if(counter < limit)
          {
              accumulator += value;              
              counter++;             
          }
          else {
              avg = accumulator / counter; 
              counter = 1;
              accumulator = 0;              
              Serial.println(avg);          
          }*/
          //char* hex = std::hex << value;          
          Serial.println(value);
          //serialBT.print(String(value) + " ");
               
          
          //Serial.println(avg);
      }
  }
  
  //serialBT.print("hola");
}
