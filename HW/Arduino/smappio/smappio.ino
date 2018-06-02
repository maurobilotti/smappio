#include "libraries/SmappioSound/SmappioSound.cpp"
#include "BluetoothSerial.h"

int *buffer;
BluetoothSerial serialBT; 
int32_t value = 0;
int framesRead = 0;
int plotterBauds = 115200;
int serialBauds = 576000;
int media = 6935;

SmappioSound smappioSound(media); // valor al azar harcodeado para nivelar a 0 la señal media


#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif


void setup() {
  // put your setup code here, to run once:

  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  serialBT.begin("smappio_PCM");
  //Serial.begin(plotterBauds);  
  Serial.begin(serialBauds);  
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  smappioSound.begin(buffer);
}

//int32_t i = -10000000;
void loop() {
  
  //gets the amount of bytes readed from the buffer. 
  
    //Serial.println(value);    

    //Little-endian ->  buf[0] menos significativo -> buf[3] más significativo
    
    /*byte buf[4];
    /*buf[0] = i & 255;
    buf[1] = (i >> 8)  & 255;
    buf[2] = (i >> 16) & 255;
    buf[3] = (i >> 24) & 255;*/
    smappioSound.read();
    value = smappioSound.getSampleValue()^2.5f;  
    Serial.write((byte *)&value, sizeof(int32_t));
}


