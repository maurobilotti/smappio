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

void SmappioSoundBufferPrinter::print(int *buffer, int len, int signalBalancer, print_mode_t printMode, bool printBothChannels)
{
    log("Print lenght", len);

    int i;
    int frame = 0;

    for (i = 0; i < len; i++)
    {
        // Se hace un corrimiento de bits a la derecha 18 posiciones, dejando a la izq un padding de ceros
        frame = (buffer[i] >> 14) & 0b00000000000000111111111111111111; 

        // frame -= 0b00000000000000101110000000000000; // signalBanalncer no funciona, balanceo la señal a mano acá

        frame += signalBalancer;

        // substraction se puede utilizar para analizar solo datos fuera de la señal estable
        int substraction = frame - 4000000000;

        if(substraction < 0 || true)
        {
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
    printf("%i\n", frame); // printf("%14d:", frame);
}