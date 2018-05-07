//  SmappioSoundBufferPrinter.h - Library for print sound buffer from I2S protocol 
//  Created by Smappio, May 6, 2018.

#include "Arduino.h"

SmappioSoundBufferPrinter::SmappioSoundBufferPrinter()
{
}

void SmappioSoundBufferPrinter::print_buffer_as_binary(int *p, int len, int signalBalancer, bool monitorMode)
{
    int i;
    int frame = 0;

    for (i = 0; i < len; i++)
    {
        if (monitorMode && (i % 2 == 0)) printf("\n");
        frame = p[i];
        frame += signalBalancer;
        printf("%14d\n", frame);
        if(monitorMode)
        {
            printf("bits:\n");
            this->printBits(sizeof(frame), &frame);
            // if (i%2 == 0) printf("|");
        }
    }
}

void SmappioSoundBufferPrinter::printBits(size_t const size, void const *const ptr)
{
    unsigned char *b = (unsigned char *)ptr;
    unsigned char byte;
    int i, j;

    for (i = size - 1; i >= 0; i--)
    {
        for (j = 7; j >= 0; j--)
        {
            byte = (b[i] >> j) & 1;
            printf("%u", byte);
        }
    }
    printf(" ");
}