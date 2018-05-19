//This example code is in the Public Domain (or CC0 licensed, at your option.)
//By Evandro Copercini - 2018
//
//This example creates a bridge between Serial and Classical Bluetooth (SPP)
//and also demonstrate that SerialBT have the same functionalities of a normal Serial

#include "BluetoothSerial.h"

const int inputPin = A3;
unsigned int inputSample;
const int inputWindow = 100;

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

BluetoothSerial SerialBT;

void setup() {
  Serial.begin(9600);
  pinMode(inputPin, INPUT);
  SerialBT.begin("smappio3"); //Bluetooth device name
  Serial.println("The device started, now you can pair it with bluetooth!");
}

void loop() {
  //SerialBT.print("hola");
  //delay(20);
  // loop for the window
  for (unsigned int i = 0; i < inputWindow; i++) {
    // read in a single value
    inputSample = analogRead(inputPin);
  }
  
  SerialBT.print(inputSample);
  
}
