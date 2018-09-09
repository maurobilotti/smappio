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
#include "Helper.h"


#pragma region // Sección de Configuración
  static const print_mode_t PRINT_MODE = INTEGER;        // Tipo de impresión: BITS | BYTES | INTEGER | FULL_DETEAILED
  static const bool PRINT_BOTH_CHANNELS = true;                                    // Indica si imprime solo el "Canal 0" ó el "Canal 0" y el "Canal 1"
  static const int FRAMES_REQUESTED = 1;                                           // Parece relacionarse con la cantidad de buffers
  static const int TICKS_TO_WAIT = 0;                                            // Investigar
  static const i2s_bits_per_sample_t BITS_PER_SAMPLE = I2S_BITS_PER_SAMPLE_32BIT;   // Datasheet: 24. Ejemplo: 32.

  static const i2s_config_t SPH_CONFIG = {
    .mode = (i2s_mode_t)(I2S_MODE_MASTER | I2S_MODE_RX),                                            // El controlador es el Maestro y Recibe, el microfono es Esclavo y Transmite
    .sample_rate = 32000,                                                                           // Entre 32KHz y 64KHz (Datasheet)
    .bits_per_sample = BITS_PER_SAMPLE,                                                             
    .channel_format = I2S_CHANNEL_FMT_ONLY_RIGHT,                                                   // El formato que usa el micrófono. No se encontró el Datasheet
    .communication_format = (i2s_comm_format_t)(I2S_COMM_FORMAT_I2S | I2S_COMM_FORMAT_I2S_MSB), //  (I2S_COMM_FORMAT_PCM)  |  (I2S_COMM_FORMAT_I2S | I2S_COMM_FORMAT_I2S_MSB)     // Investigar
    //MSB: most significant byte -> BIG ENDIAN
    .intr_alloc_flags = ESP_INTR_FLAG_LEVEL1,                                                       // High interrupt priority (investigar)
    .dma_buf_count = 2,                                                                            // Cantidad de buffers, 128 max.
    .dma_buf_len = 8                                                                           // Tamaño de cada buffer
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

    /*
       Obtiene el valor del sample 'index' del buffer
    */
    int32_t getSampleValue();

  private:
    SmappioSoundBufferPrinter bufferPrinter;      // Instancia requerida para hacer impresiones
    int _signalBalancer;                          // Número que nivela los valores de la señal (gráficamente lleva aproxima la señal a 0)
    int *_buffer;                                 // Buffer donde se almacenan los datos obtenidos por el micrófono    
};

#pragma region // Constantes (no debería modificarse esta sección)
  static const i2s_port_t CHANNEL_NUMBER = I2S_NUM_0;      // Es el canal a usar, necesario para transmisión stereo. Como usamos un solo micrófono se puede usar cualquiera
#pragma endregion Constantes

#pragma region // Configuración física (no debe modificarse si no se cambian las conexiones)

  static const int TX_PIN = 17;
  static const int RX_PIN = 16;
  static const int DATA_PIN = 21;

  static const i2s_pin_config_t SPH_PINS = {
    .bck_io_num = TX_PIN,
    .ws_io_num = RX_PIN,
    .data_out_num = I2S_PIN_NO_CHANGE,  // Fuerza a que no se modifique el pin durante la ejecución
    .data_in_num = DATA_PIN
  };

#pragma endregion Configuración física

#endif