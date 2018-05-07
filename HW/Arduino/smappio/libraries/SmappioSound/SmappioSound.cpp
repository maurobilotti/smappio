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
    int bufferSize = FRAMES_REQUESTED * I2S_BITS_PER_SAMPLE_32BIT;  // bufferSize esta medido en cantidad de bytes

    _buffer = readBuffer;
    _buffer = (int *)malloc(bufferSize);

    i2s_driver_install(I2S_NUM_0, &SPH_CONFIG, 0, NULL);            // Instalar el driver tambien inicializa la escucha
    i2s_set_pin(I2S_NUM_0, &SPH_PINS);                              // Setea la conexión física del micrófono al controlador
}

#pragma endregion Métodos básicos

int SmappioSound::read()
{
    return i2s_read_bytes(I2S_NUM_0, (char *)_buffer, FRAMES_REQUESTED, TICKS_TO_WAIT) / I2S_BITS_PER_SAMPLE_32BIT;
}

void SmappioSound::print(int len)
{
    bufferPrinter.print_buffer_as_binary(_buffer, len, _signalBalancer, PRINT_DETAILED_MODE);
}