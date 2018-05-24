#include "libraries/SmappioSound/SmappioSound.cpp"
#include "BluetoothSerial.h"

int *buffer;
int framesRead = 0;
BluetoothSerial serialBT; 

SmappioSound smappioSound(0); // valor al azar harcodeado para nivelar a 0 la señal media


#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif


void setup() {
  // put your setup code here, to run once:

  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  serialBT.begin("smappio4");
  Serial.begin(115200);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  smappioSound.begin(buffer);
}

void loop() {
  // put your main code here, to run repeatedly:
  
  framesRead = smappioSound.read() / 32;
  log("Frames read", framesRead);    
  smappioSound.print(framesRead, serialBT);
  //serialBT.print("hola");
}
