//  SmappioSoundBufferPrinter.h - Library for print sound buffer from I2S protocol 
//  Created by Smappio, May 6, 2018.

#include "Arduino.h"
#include "SmappioSoundBufferPrinter.h"

SmappioSoundBufferPrinter::SmappioSoundBufferPrinter()
{
}

void SmappioSoundBufferPrinter::print_buffer_as_binary(int *p, int len, int signalBalancer, print_mode_t printMode)
{
    log("Print lenght", len);

    int i;
    int frame = 0;

    for (i = 0; i < len; i++)
    {
        frame = p[i];
        frame += signalBalancer;
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
                printf("%d\n", frame); // printf("%14d:", frame);
                break;
            case FULL_DETEAILED:
                // Iteration
                printf("Iteración: %d\n", i);

                // Entero
                printf("Entero:    %d\n", frame); // printf("%14d:", frame);
                
                // Bits
                printf("Bits:      ");
                this->printBits(sizeof(frame), &frame); //revisar el sizeof
                if (i%2 == 0) printf("|");
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

void SmappioSoundBufferPrinter::printBits(size_t const size, void const *const ptr)
{
    unsigned char *b = (unsigned char *)ptr;  // [unsigned char] = 8 bits
    unsigned char byte;
    int i, j;

    for (i = 0; i < size; i++)
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
    size_t i;
    int byte;
    for (i = 0; i < len; ++i) {
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