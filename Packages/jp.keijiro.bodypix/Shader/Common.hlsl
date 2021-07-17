#ifndef _BODYPIXBARRACUDA_COMMON_H_
#define _BODYPIXBARRACUDA_COMMON_H_

#define PART_NOSE           0
#define PART_LEFT_EYE       1
#define PART_RIGHT_EYE      2
#define PART_LEFT_EAR       3
#define PART_RIGHT_EAR      4
#define PART_LEFT_SHOULDER  5
#define PART_RIGHT_SHOULDER 6
#define PART_LEFT_ELBOW     7
#define PART_RIGHT_ELBOW    8
#define PART_LEFT_WRIST     9
#define PART_RIGHT_WRIST    10
#define PART_LEFT_HIP       11
#define PART_RIGHT_HIP      12
#define PART_LEFT_KNEE      13
#define PART_RIGHT_KNEE     14
#define PART_LEFT_ANKLE     15
#define PART_RIGHT_ANKLE    16

#define PART_COUNT          17

float Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

#endif
