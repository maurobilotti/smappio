#include "libraries/SmappioSound/SmappioSound.cpp"
#include "BluetoothSerial.h"

int *buffer;
BluetoothSerial serialBT; 
int32_t value = 0;
int bytesReaded = 0;
int plotterBauds = 115200;
int serialBauds = 576000;
int media = 6835;  // 13700  |  6835
int samplesCounter = 0;
int samplesLimit = 32000 * 30;
long time1 = 0;
long time2 = 0;

SmappioSound smappioSound(media); // valor al azar harcodeado para nivelar a 0 la señal media


#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif


void setup() {
  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  Serial.begin(2000000);  //   115200   |   900000   |   2000000
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  smappioSound.begin(buffer);
  time1 = millis();
}

//int32_t i = -10000000;
void loop() {
  
  //gets the amount of bytes readed from the buffer. 
  

    //Little-endian ->  buf[0] menos significativo -> buf[3] más significativo
    
    /*byte buf[4];
    /*buf[0] = i & 255;
    buf[1] = (i >> 8)  & 255;
    buf[2] = (i >> 16) & 255;
    buf[3] = (i >> 24) & 255;*/
    if(samplesCounter < samplesLimit)
    {
      bytesReaded = smappioSound.read();
      value = smappioSound.getSampleValue();  
      // samplesCounter += bytesReaded / 4;
      Serial.write((byte *)&value, sizeof(int32_t));
      Serial.flush();
      //smappioSound.print(bytesReaded);    
      //printf("%d\n", samplesCounter);
    }
    else {
      time2 = millis();
      Serial.print("Tiempo transcurrido: ");
      Serial.println((time2 - time1));
      while(1);
    }
}


