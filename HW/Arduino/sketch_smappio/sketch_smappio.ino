#include "driver/i2s.h"
#include "freertos/queue.h"

static const int TX_PIN = 17;
static const int RX_PIN = 16;

int buffer_size_in_bytes;
int frames_read;
static const int FRAMES_REQUESTED = 32; // Esto creo que va de la mano con la cantidad de buffers
bool avaiable = true;

// Información no sacada del Datasheet obtenida de: https://www.esp32.com/viewtopic.php?t=1506
static const i2s_config_t SPH_CONFIG = {
   .mode = (i2s_mode_t)(I2S_MODE_MASTER | I2S_MODE_RX), // El controlador es el Maestro y Recibe, el microfono es Esclavo y Transmite
   .sample_rate = 32000, // Entre 32KHz y 64KHz (Datasheet)
   .bits_per_sample = I2S_BITS_PER_SAMPLE_32BIT, // Creo que son 32 (8 del MSB y 24 de Datos)
   .channel_format = I2S_CHANNEL_FMT_RIGHT_LEFT,
   .communication_format = (i2s_comm_format_t)(I2S_COMM_FORMAT_I2S | I2S_COMM_FORMAT_I2S_MSB),
   .intr_alloc_flags = ESP_INTR_FLAG_LEVEL1, // high interrupt priority
   .dma_buf_count = 14, // Cantidad de buffers, 128 max.
   .dma_buf_len = 32*2 // Tamaño de cada buffer
};

static const i2s_pin_config_t SPH_PINS = {
   .bck_io_num = TX_PIN,
   .ws_io_num = RX_PIN,
   .data_out_num = I2S_PIN_NO_CHANGE,
   .data_in_num = 14
   };

void setup() {
  // put your setup code here, to run once:

  buffer_size_in_bytes = FRAMES_REQUESTED * I2S_BITS_PER_SAMPLE_32BIT;
  frames_read = 0;
  
  i2s_driver_install(I2S_NUM_0, &SPH_CONFIG, 0, NULL);
  i2s_set_pin(I2S_NUM_0, &SPH_PINS);
}

void loop() {
  // put your main code here, to run repeatedly:

  if(avaiable) {
    TickType_t ticks_to_wait = 1000;
    int *buffer = (int*)malloc(buffer_size_in_bytes);
    int i;
    // Skip initial frames
    for(i = 0; i < 6; i++) {
        frames_read = i2s_read_bytes(I2S_NUM_0, (char*)buffer, FRAMES_REQUESTED, 0) / I2S_BITS_PER_SAMPLE_32BIT;
    }
  
    // Main loop
    for(i = 0; i < 100; i++) {
        frames_read = i2s_read_bytes(I2S_NUM_0, (char*)buffer, FRAMES_REQUESTED, ticks_to_wait) / I2S_BITS_PER_SAMPLE_32BIT;
  
       print_buffer_as_binary(buffer, frames_read);   
    }
    
    Serial.println("Done! \n");
    // avaiable = false;
  }
}

void print_buffer_as_binary(int *p, int len)
{
    int i;
    int frame = 0;

    for (i = 0; i < len; i++) {
        // if (i%2 == 0) printf("\n");
        frame = p[i];
        frame += 223837953;
        //Serial.println(frame);
        printf("%14d\n", frame);
        // printBits(sizeof(frame), &frame);
        // if (i%2 == 0) printf("|");
    }
}

void printBits(size_t const size, void const * const ptr)
{
    unsigned char *b = (unsigned char*) ptr;
    unsigned char byte;
    int i, j;

    for (i=size-1;i>=0;i--)
    {
        for (j=7;j>=0;j--)
        {
            byte = (b[i] >> j) & 1;
            printf("%u", byte);
        }
    }
    printf(" ");
}
