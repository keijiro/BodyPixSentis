#ifndef _BODYPIXBARRACUDA_COMMON_H_
#define _BODYPIXBARRACUDA_COMMON_H_

#define KEYPOINT_NOSE           0
#define KEYPOINT_LEFT_EYE       1
#define KEYPOINT_RIGHT_EYE      2
#define KEYPOINT_LEFT_EAR       3
#define KEYPOINT_RIGHT_EAR      4
#define KEYPOINT_LEFT_SHOULDER  5
#define KEYPOINT_RIGHT_SHOULDER 6
#define KEYPOINT_LEFT_ELBOW     7
#define KEYPOINT_RIGHT_ELBOW    8
#define KEYPOINT_LEFT_WRIST     9
#define KEYPOINT_RIGHT_WRIST    10
#define KEYPOINT_LEFT_HIP       11
#define KEYPOINT_RIGHT_HIP      12
#define KEYPOINT_LEFT_KNEE      13
#define KEYPOINT_RIGHT_KNEE     14
#define KEYPOINT_LEFT_ANKLE     15
#define KEYPOINT_RIGHT_ANKLE    16

#define KEYPOINT_COUNT          17

float Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

#endif
