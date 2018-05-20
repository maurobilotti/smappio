//  SmappioSoundBufferPrinter.h - Library for print sound buffer from I2S protocol 
//  Created by Smappio, May 6, 2018.

#include "Arduino.h"
#include "SmappioSoundBufferPrinter.h"

SmappioSoundBufferPrinter::SmappioSoundBufferPrinter()
{
}

void SmappioSoundBufferPrinter::print(int *p, int len, int signalBalancer, print_mode_t printMode, bool printBothChannels)
{
    log("Print lenght", len);

    int i;
    int frame = 0;

    for (i = 0; i < len; i++)
    {
        frame = p[i];
        frame += signalBalancer;
        if(printBothChannels || i%2 == 0)
        {
            switch(printMode)
            {
                case BITS:
                    this->printBits(sizeof(frame), &frame); //revisar el sizeof
                    if (i%2 == 0) printf("|");
                    break;
                case BYTES:
                    this->printBytes(sizeof(frame), &frame); //revisar el sizeof
                    break;
                case INTEGER:
                    this->printInteger(frame);
                    break;
                case FULL_DETEAILED:
                    if (i%2 == 0) 
                        printf("--- CANAL 0 ---\n");
                    else
                        printf("--- CANAL 1 ---\n");

                    // Iteration
                    printf("Iteración: %d\n", i);

                    // Entero
                    printf("Entero:    ", frame);
                    this->printInteger(frame);
                    
                    // Bits
                    printf("Bits:      ");
                    this->printBits(sizeof(frame), &frame); //revisar el sizeof
                    printf("\n");
                    
                    // Bytes
                    printf("Bytes:     ");
                    this->printBytes(sizeof(frame), &frame); //revisar el sizeof
                    printf("\n\n");
                    break;
                default:
                    printf("Tipo de impresión no definido");
                    break;
                printf("\n");
            }
        }
    }
}

void SmappioSoundBufferPrinter::printBits(size_t const size, void const *const ptr)
{
    unsigned char *b = (unsigned char *)ptr;  // [unsigned char] = 8 bits
    unsigned char byte;
    int i, j;

    for (i = size - 1; i >= 0; i--)
    {
        for (j = 7; j >= 0; j--)
        {
            byte = (b[i] >> j) & 1;
            printf("%u", byte);
        }
        printf("  ");   // 2 spaces to separate bytes
    }
    printf(" ");
}

void SmappioSoundBufferPrinter::printBytes(size_t len, void *p)
{
    int i, byte;

    for (i = len - 1; i >= 0; i--) {
        printf("  ");                   // 2 spaces padding left

        byte = ((char*)p)[i];

        if ( byte < 99) 
            printf(" ");                // Add 1 space padding left

        printf("%d", byte);

        printf("   ");                  // 3 spaces padding right
        
        if( byte < 10)                  
            printf(" ");                // Add 1 space padding right

        printf("  ");                   // 2 spaces to separate bytes
    }
}

void SmappioSoundBufferPrinter::printInteger(int frame)
{
    printf("%u\n", frame); // printf("%14d:", frame);
}