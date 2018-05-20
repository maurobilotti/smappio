//  SmappioSoundBufferPrinter.h - Library for print sound buffer from I2S protocol 
//  Created by Smappio, May 6, 2018.

#ifndef SmappioSoundBufferPrinter_h
#define SmappioSoundBufferPrinter_h

#include "Arduino.h"
#include "Helper.h"

typedef enum {
    BITS            = 0,       // Solo bits
    BYTES           = 1,       // Solo bytes
    BINARY          = 2,       // Solo resultado binario
    FULL_DETEAILED  = 3,       // El resultado formateado de todas las formas y detallado
} print_mode_t;

class SmappioSoundBufferPrinter
{
  public:
    SmappioSoundBufferPrinter();
    void print_buffer_as_binary(int *p, int len, int signalBalancer, print_mode_t printMode);
    void debug(char* msg);
    
  private:
    void printBits(size_t const size, void const *const p);
    void printBytes(size_t len, void *ptr);
};


#endif