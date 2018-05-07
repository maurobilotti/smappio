#include "libraries/SmappioSound/SmappioSound.cpp"

int *buffer;
int framesRead = 0;

SmappioSound smappioSound(223837953); // 223837953 valor al azar harcodeado para nivelar a 0 la señal media

void setup() {
  // put your setup code here, to run once:

  smappioSound.begin(buffer);
}

void loop() {
  // put your main code here, to run repeatedly:
  
  framesRead = smappioSound.read();    
  smappioSound.print(framesRead);
}
