#include <I2S.h>

const long samples = 100000000;
unsigned long initTime = 0;
long samplesCounter = 0;

unsigned long tiempo1 = 0;
unsigned long tiempo2 = 0; 
unsigned long tiempoSegundos = 0;

void setup() {

  Serial.begin(115200);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  I2S.onReceive(onI2SReceive);
  
  int sampleRate = 31250; // 48,000,000 / sampleRate must be a multiple of 64
  if (!I2S.begin(I2S_PHILIPS_MODE, sampleRate, 32)) {
    Serial.println("Failed to initialize I2S!");
    while (1); // do nothing
  }
  
  initTime = millis();
  tiempo1 = millis();
  samplesCounter += 64;  // 256 bytes of channel / 4 bytes per sample
  I2S.read(); // Needed to start the interrupt handler  
}

uint8_t buffer[512];
volatile int available = 0;
volatile int read = 0;

void onI2SReceive() {
  // This function will run at a frequency of (sampleRate / 64)
  // At 31.25khz, this is every 1962.5 microseconds so make sure any processing here takes less
  // time than that

  I2S.read(buffer, 512); // Will actually read 256 bytes
  // Do analysis here, but make sure it completes before the next batch samples comes in
  available = 1;
  samplesCounter += 64;  // 256 bytes of channel / 4 bytes per sample
}

void loop() {
  tiempo2 = millis();

  if (available) {
    int *values = (int *) buffer;
    for (int i = 0; i < 64; i++) {
      // If the SEL pin is low, samples will be in odd numbered positions.
      // If you connect SEL to high, data will be in even positions.
      Serial.print((values[(2*i) + 1] >> 14) + 10000);
      Serial.print("|");
    }
    available = 0;
  }

  if(tiempo2 > (tiempo1+1000)){  //Si ha pasado 1 segundo ejecuta el IF
    
    tiempo1 = millis(); //Actualiza el tiempo actual
    tiempoSegundos = tiempo1/1000;
    //printf("\nHa transcurrido: %d desde que se encendio el Arduino. Cantidad: %d\n", tiempoSegundos, samplesCounter);
    Serial.println("");
    Serial.print("Ha transcurrido: ");
    Serial.print(tiempoSegundos);
    Serial.print(" desde que se encendio el Arduino");
    Serial.print("Cantidad:");
    Serial.println(samplesCounter);
    
    samplesCounter = 0;
  }

  // if(samplesCounter >= samples) {
  //   int seconds = (millis() - initTime) / 1000;
  //   Serial.print("\nSegundos transcurridos: ");
  //   Serial.println(seconds);
  //   Serial.print("Muestras por segundo: ");
  //   Serial.println(samples / seconds);
  //   while(1);
  // }
}
