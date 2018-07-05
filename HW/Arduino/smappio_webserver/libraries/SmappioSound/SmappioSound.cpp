//  SmappioSound.h - Library for use I2S protocol betweeen SPH0645 Microphone and Adafruit Huzzah ESP32.
//  Created by Smappio, May 6, 2018.
//////////////////////////////////////////////////////////////////
//    Toda información no sacada del Datasheet obtenida de:     //
//    https://www.esp32.com/viewtopic.php?t=1506                //
//////////////////////////////////////////////////////////////////

#include "Arduino.h"
#include "driver/i2s.h"
#include "freertos/queue.h"
#include "SmappioSound.h"

#pragma region // Métodos básicos

SmappioSound::SmappioSound(int signalBalancer)
{
    _signalBalancer = signalBalancer;
}

void SmappioSound::begin(int *readBuffer)
{
    //int startingTime = 3000;
    int bufferSize = FRAMES_REQUESTED * (BITS_PER_SAMPLE / 8);  // bufferSize esta medido en cantidad de bytes

    // log("Frames requested", FRAMES_REQUESTED);
    // log("Bits per sample", BITS_PER_SAMPLE);
    // log("Buffer size", bufferSize);

    _buffer = readBuffer;
    _buffer = (int *)malloc(bufferSize);

    i2s_driver_install(CHANNEL_NUMBER, &SPH_CONFIG, 0, NULL);            // Instalar el driver tambien inicializa la escucha
    i2s_set_pin(CHANNEL_NUMBER, &SPH_PINS);                              // Setea la conexión física del micrófono al controlador
    //i2s_set_sample_rates(CHANNEL_NUMBER, 30000); //set sample rates
    
    // log("Waiting seconds for initialization", startingTime / 1000);
    // delay(startingTime);
}

#pragma endregion Métodos básicos

int SmappioSound::read()
{
    int bytesReaded = i2s_read_bytes(CHANNEL_NUMBER, (char *)_buffer, 4, TICKS_TO_WAIT);

    //log("\nBytes pushed to DMA buffer", bytesReaded);

    return bytesReaded;
}

void SmappioSound::print(int len)
{
    bufferPrinter.print(_buffer, len, _signalBalancer, PRINT_MODE, PRINT_BOTH_CHANNELS);
}

int32_t SmappioSound::getSampleValue()
{
    return bufferPrinter.getSampleValue(_buffer, _signalBalancer, PRINT_BOTH_CHANNELS);
}