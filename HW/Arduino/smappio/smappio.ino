#include "libraries/SmappioSound/SmappioSound.h"
#include "libraries/SmappioSound/SmappioSound.cpp"

int *unBuffer;
int framesRead = 0;

SmappioSound smappioSound(14, 223837953);

void setup() {
  // put your setup code here, to run once:

  smappioSound.begin(unBuffer);
}

void loop() {
  // put your main code here, to run repeatedly:
  
  framesRead = smappioSound.read();    
  smappioSound.print(buffer, framesRead);
}
