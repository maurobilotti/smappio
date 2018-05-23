#include <SoftwareSerial.h>
/*
big endian
16 bit depth
256hz sample rate
channels: 1 mono
*/


SoftwareSerial connection(10,11);
int microphonePin = 0;
int samples = 0;
int flag = 0;
void setup() {
  // put your setup code here, to run once:
  pinMode(microphonePin, INPUT);
  connection.begin(9600);
  Serial.begin(9600); 
}

void loop() {
  // put your main code here, to run repeatedly: 
  int response = analogRead(microphonePin);      
  Serial.println(response);
  connection.print(String(response) + " ");
  delay(1);  
}


