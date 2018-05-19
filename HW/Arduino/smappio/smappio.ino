#include "libraries/SmappioSound/SmappioSound.cpp"

int *buffer;
int framesRead = 0;

SmappioSound smappioSound(223837953); // 223837953 valor al azar harcodeado para nivelar a 0 la señal media

void setup() {
  // put your setup code here, to run once:

  pinMode(DATA_PIN, INPUT); // Supuestamente necesario para que no haya ruido
  Serial.begin(115200);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB port only
  }

  smappioSound.begin(buffer);
}

void loop() {
  // put your main code here, to run repeatedly:
  
  framesRead = smappioSound.read();    
  smappioSound.print(framesRead);
}
