//  SmappioSoundBufferPrinter.h - Library for print sound buffer from I2S protocol 
//  Created by Smappio, May 6, 2018.

#include "Arduino.h"
#include "SmappioSoundBufferPrinter.h"

SmappioSoundBufferPrinter::SmappioSoundBufferPrinter()
{	
}

int32_t SmappioSoundBufferPrinter::getSampleValue(int *buffer, int signalBalancer, bool printBothChannels)
{
    int32_t sample = buffer[0];
    sample >>= 14;  //2^15  
    sample += signalBalancer;    
    // sample = sample << 8 & 0b00000011111111111111111100000000; // En el sketch, el ultimo byte en 0 no se transmite

    // Entero
    // printf("Entero:    ", sample);
    // this->printInteger(sample);
    
    // // Bits
    // printf("Bits:      ");
    // this->printBits(sizeof(sample), &sample); //revisar el sizeof
    // printf("\n");
    
    // // Bytes
    // printf("Bytes:     ");
    // this->printBytes(sizeof(sample), &sample); //revisar el sizeof
    // printf("\n\n");    
    
    return sample;
}

void SmappioSoundBufferPrinter::print(int *buffer, int bytesLen, int signalBalancer, print_mode_t printMode, bool printBothChannels)
{
    log("\nBytes lenght", bytesLen);

    int i;
    int sample = 0;
    
    for (i = 0; i < bytesLen / 4; i++)
    {
        if (i%2 == 0) 
            log("\n--- CANAL 0 ---");
        else
            log("\n--- CANAL 1 ---");

        // Se hace un corrimiento de bits a la derecha 18 posiciones
        sample = buffer[i] >> 14; 

        sample += signalBalancer;

        if(printBothChannels || i%2 == 0)
        {
            switch(printMode)
            {
                case BITS:
                    if (i%2 == 0) printf("                                        ");
                    this->printBits(sizeof(sample), &sample); //revisar el sizeof
                    break;
                case BYTES:
                    this->printBytes(sizeof(sample), &sample); //revisar el sizeof
                    break;
                case INTEGER:
                    this->printInteger(sample);                        
                    break;
                case FULL_DETEAILED:
                    // Iteration
                    printf("Iteración: %d\n", i);

                    // Entero
                    printf("Entero:    ", sample);
                    this->printInteger(sample);
                    
                    // Bits
                    printf("Bits:      ");
                    this->printBits(sizeof(sample), &sample); //revisar el sizeof
                    printf("\n");
                    
                    // Bytes
                    printf("Bytes:     ");
                    this->printBytes(sizeof(sample), &sample); //revisar el sizeof
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

void SmappioSoundBufferPrinter::printInteger(int sample)
{
    printf("%d ",131072);
    printf("%i ", sample);
    printf("%d ",-131072);

    printf("\n");
}