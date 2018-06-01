#include "libraries/SmappioSound/SmappioSound.cpp"
#include "BluetoothSerial.h"

int *buffer;
BluetoothSerial serialBT; 
int32_t value = 0;
int framesRead = 0;
int plotterBauds = 115200;
int serialBauds = 960000;
int media = 6880;

SmappioSound smappioSound(media); // valor al azar harcodeado para nivelar a 0 la señal media


#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif


void setup() {
  // put your setup code here, to run once:

  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  serialBT.begin("smappio_PCM");
  Serial.begin(serialBauds);  
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  smappioSound.begin(buffer);
}


void loop() {
  
  //gets the amount of bytes readed from the buffer. 
    smappioSound.read();
    value = smappioSound.getSampleValue();  
    //Serial.println(value);

    byte buf[4];
    buf[0] = value & 255;
    buf[1] = (value >> 8)  & 255;
    buf[2] = (value >> 16) & 255;
    buf[3] = (value >> 24) & 255;
    
    Serial.write(buf, sizeof(int32_t));            
}


