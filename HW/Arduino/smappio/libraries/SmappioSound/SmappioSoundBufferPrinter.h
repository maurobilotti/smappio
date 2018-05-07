//  SmappioSoundBufferPrinter.h - Library for print sound buffer from I2S protocol 
//  Created by Smappio, May 6, 2018.

#ifndef SmappioSoundBufferPrinter_h
#define SmappioSoundBufferPrinter_h

#include "Arduino.h"

class SmappioSoundBufferPrinter
{
  public:
    SmappioSoundBufferPrinter();
    void print_buffer_as_binary(int *p, int len, int signalBalancer, bool monitorMode);
    
  private:
    void printBits(size_t const size, void const *const ptr);
};

#endif