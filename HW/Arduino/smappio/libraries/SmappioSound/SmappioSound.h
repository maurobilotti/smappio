/*
  SmappioSound.h - Library for use I2S protocol betweeen SPH0645 Microphone and Adafruit Huzzah ESP32.
  Created by Smappio, May 6, 2018.
*/
#ifndef SmappioSound_h
#define SmappioSound_h

#include "Arduino.h"

class SmappioSound
{
  public:
    SmappioSound(int pin, int signalBalancer);
    void begin(int *readBuffer);
    int read();
    void print(int *buffer, int len);
  private:
    int _pin;
    int _signalBalancer;
    void print_buffer_as_binary(int *p, int len, bool printBits);
    void printBits(size_t const size, void const * const ptr);
};

#endif