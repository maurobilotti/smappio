#include <SoftwareSerial.h>

SoftwareSerial connection(10,11);

char bufferData[] = "0123456789";
int lowerLimit = 0;
int upperLimit = 10;

void setup() {
  // put your setup code here, to run once:
  connection.begin(9600);
  Serial.begin(9600);
 
}

void loop() {
  // put your main code here, to run repeatedly:
  char dataToTransmit[10];
  int i = 0;
    
  for(lowerLimit; lowerLimit < upperLimit; lowerLimit++)
  {
      dataToTransmit[i] = bufferData[lowerLimit];
      Serial.print(dataToTransmit[lowerLimit]);
      i++;  
  }

  lowerLimit = 0;  
  connection.print(dataToTransmit);
}


