#include "libraries/SmappioSound/SmappioSound.cpp"

// CONSTANTES
#define BYTES_TO_SEND 3
#define MEDIA 13700   // I2S:  13700  | PCM:  6835   // valor para nivelar a 0 la señal media

// VARIABLES
int32_t *_buffer;
uint8_t _dataToSend[BYTES_TO_SEND];
SmappioSound smappioSound(MEDIA); 

void setup() {
  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  Serial.begin(2000000);  //   115200   |   900000   |   2000000
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  smappioSound.begin(_buffer);
}

void loop() 
{
  if(Serial.available())
  {
    char c = (char)Serial.read();
    if(c == 's') // Si el cliente mando la señal send ('s'), se empieza a transmitir, caso contrario, ignora dicha señal
    {  
      while(1) 
      { // Se queda transmitiendo por siempre, hasta que se aprete el boton
        bufferSamplesToSendWithControlBits();
        Serial.write(_dataToSend, BYTES_TO_SEND);
      }
    }
  }
}


// Metodo para bufferear sonido con bits de control
void bufferSamplesToSendWithControlBits() 
{
  int32_t value = 0;
  int bytesReaded = 0;

  for(int i = 0; i < BYTES_TO_SEND; i += 3)
  {
    // Se hace la lectura de los samples del microfono
    bytesReaded = smappioSound.read();
    while(bytesReaded == 0)
    {     
        //Este bucle es necesario para no reenviar una muestra mas de una vez
        bytesReaded = smappioSound.read();
    }   
    // Se lee un sample
    value = smappioSound.getSampleValue();
    
    // Se bufferean los 3 bytes del sample, con un desplazamiento y una numeracion
    // 'a': bit del primer byte. 'b': bit del segundo byte. 'c': bit del tercer byte.
    _dataToSend[i] = (value & 63) | 64;               // '64'   =  01|aaaaaa
    _dataToSend[i + 1] = ((value >> 6)  & 63) | 128;  // '128'  =  10|bbbbaa
    _dataToSend[i + 2] = ((value >> 12) & 63) | 192;  // '192'  =  11|ccbbbb
  }  
}

// Metodo para bufferear sonido sin bits de control
void bufferSamplesToSend() 
{
  int32_t value = 0;
  int bytesReaded = 0;

  for(int i = 0; i < BYTES_TO_SEND; i += 3)
  {
    // Se hace la lectura de los samples del microfono
    bytesReaded = smappioSound.read();
    while(bytesReaded == 0)
    {     
        //Este bucle es necesario para no reenviar una muestra mas de una vez
        bytesReaded = smappioSound.read();
    }   
    // Se lee un sample
    value = smappioSound.getSampleValue();

    // Se bufferean los 3 bytes del sample
    _dataToSend[i] = value & 255;
    _dataToSend[i + 1] = (value >> 8)  & 255;
    _dataToSend[i + 2] = (value >> 16) & 255; 
  }  
}