#ifndef _BODYPIXBARRACUDA_COMMON_H_
#define _BODYPIXBARRACUDA_COMMON_H_

#define PART_COUNT 17

float Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

#endif
