//  SmappioSound.h - Library for use I2S protocol betweeen SPH0645 Microphone and Adafruit Huzzah ESP32.
//  Created by Smappio, May 6, 2018.
//////////////////////////////////////////////////////////////////
//    Toda información no sacada del Datasheet obtenida de:     //
//    https://www.esp32.com/viewtopic.php?t=1506                //
//////////////////////////////////////////////////////////////////

#ifndef SmappioSound_h
#define SmappioSound_h

#include "Arduino.h"
#include "SmappioSoundBufferPrinter.h"
#include "SmappioSoundBufferPrinter.cpp"


#pragma region // Sección de Configuración

  static const bool PRINT_DETAILED_MODE = false;    // true: Impresión detallada del buffer. false: Impresión del buffer como números enteros (usar este último modo para el plotter)
  static const int FRAMES_REQUESTED = 32;           // Parece relacionarse con la cantidad de buffers
  static const int TICKS_TO_WAIT = 1000;            // Investigar

  static const i2s_config_t SPH_CONFIG = {
    .mode = (i2s_mode_t)(I2S_MODE_MASTER | I2S_MODE_RX),                                            // El controlador es el Maestro y Recibe, el microfono es Esclavo y Transmite
    .sample_rate = 32000,                                                                           // Entre 32KHz y 64KHz (Datasheet)
    .bits_per_sample = I2S_BITS_PER_SAMPLE_32BIT,                                                   // Creo que son 32 (8 del MSB y 24 de Datos)
    .channel_format = I2S_CHANNEL_FMT_RIGHT_LEFT,                                                   // El formato que usa el micrófono. No se encontró el Datasheet
    .communication_format = (i2s_comm_format_t)(I2S_COMM_FORMAT_I2S | I2S_COMM_FORMAT_I2S_MSB),     // Investigar
    .intr_alloc_flags = ESP_INTR_FLAG_LEVEL1,                                                       // High interrupt priority (investigar)
    .dma_buf_count = 14,                                                                            // Cantidad de buffers, 128 max.
    .dma_buf_len = 32 * 2                                                                           // Tamaño de cada buffer
  };

#pragma endregion Sección de Configuración

class SmappioSound
{
  public:

    /**
     * @brief Constructor
     *
     * @param signalBalancer  Número que nivela los valores de la señal (gráficamente lleva aproxima la señal a 0)
     *
     * @return Instancia de la clase
     */
    SmappioSound(int signalBalancer);

    /**
     * @brief Instala el driver, aloca memoria para el buffer y inicia la escucha
     * *
     * @param readBuffer buffer donde se va a almacenar la información
     *
     */
    void begin(int *readBuffer);

    /**
     * @brief Read data from I2S DMA receive buffer - DMA: Direct Memmory Access: No interviene el procesador
     *
     * @param signalBalancer  Número que nivela los valores de la señal (gráficamente lleva aproxima la señal a 0)
     *
     * @return
     *     - Número de bytes leídos.
     *     - ESP_FAIL  Parameter error.
     */
    int read(); 

    /**
     * @brief Imprime la cantidad indicada de Frames en el puerto Serie
     *
     */    
    void print(int len);

  private:
    SmappioSoundBufferPrinter bufferPrinter;      // Instancia requerida para hacer impresiones
    int _signalBalancer;                          // Número que nivela los valores de la señal (gráficamente lleva aproxima la señal a 0)
    int *_buffer;                                 // Buffer donde se almacenan los datos obtenidos por el micrófono
};

#pragma region // Configuración física (no debe modificarse si no se cambian las conexiones)

  static const int TX_PIN = 17;
  static const int RX_PIN = 16;
  static const int DATA_PIN = 14;

  static const i2s_pin_config_t SPH_PINS = {
    .bck_io_num = TX_PIN,
    .ws_io_num = RX_PIN,
    .data_out_num = I2S_PIN_NO_CHANGE,  // Fuerza a que no se modifique el pin durante la ejecución
    .data_in_num = DATA_PIN
  };

#pragma endregion Configuración física

#endif