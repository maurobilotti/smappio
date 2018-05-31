
#ifndef helper_h
#define helper_h

#include "Arduino.h"

const bool DEBUG_MODE = true;

void log(char* msg);
void log(char* msg, int value);
void log(char* msg, double value);

void log(char* msg)
{
    if(DEBUG_MODE)
        printf("%s\n", msg);
}

void log(char* msg, int value)
{
    if(DEBUG_MODE)
        printf("%s: %d\n", msg, value);
}

void log(char* msg, double value)
{
    if(DEBUG_MODE)
        printf("%s: %f\n", msg);
}

#endif