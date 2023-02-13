#ifndef _BODYPIX_BARRACUDA_COMMON_H_
#define _BODYPIX_BARRACUDA_COMMON_H_

// Keypoint IDs
#define BODYPIX_KEYPOINT_NOSE               0
#define BODYPIX_KEYPOINT_LEFT_EYE           1
#define BODYPIX_KEYPOINT_RIGHT_EYE          2
#define BODYPIX_KEYPOINT_LEFT_EAR           3
#define BODYPIX_KEYPOINT_RIGHT_EAR          4
#define BODYPIX_KEYPOINT_LEFT_SHOULDER      5
#define BODYPIX_KEYPOINT_RIGHT_SHOULDER     6
#define BODYPIX_KEYPOINT_LEFT_ELBOW         7
#define BODYPIX_KEYPOINT_RIGHT_ELBOW        8
#define BODYPIX_KEYPOINT_LEFT_WRIST         9
#define BODYPIX_KEYPOINT_RIGHT_WRIST        10
#define BODYPIX_KEYPOINT_LEFT_HIP           11
#define BODYPIX_KEYPOINT_RIGHT_HIP          12
#define BODYPIX_KEYPOINT_LEFT_KNEE          13
#define BODYPIX_KEYPOINT_RIGHT_KNEE         14
#define BODYPIX_KEYPOINT_LEFT_ANKLE         15
#define BODYPIX_KEYPOINT_RIGHT_ANKLE        16
#define BODYPIX_KEYPOINT_COUNT              17

// Part IDs
#define BODYPIX_PART_LEFT_FACE              0
#define BODYPIX_PART_RIGHT_FACE             1
#define BODYPIX_PART_LEFT_UPPER_ARM_FRONT   2
#define BODYPIX_PART_LEFT_UPPER_ARM_BACK    3
#define BODYPIX_PART_RIGHT_UPPER_ARM_FRONT  4
#define BODYPIX_PART_RIGHT_UPPER_ARM_BACK   5
#define BODYPIX_PART_LEFT_LOWER_ARM_FRONT   6
#define BODYPIX_PART_LEFT_LOWER_ARM_BACK    7
#define BODYPIX_PART_RIGHT_LOWER_ARM_FRONT  8
#define BODYPIX_PART_RIGHT_LOWER_ARM_BACK   9
#define BODYPIX_PART_LEFT_HAND              10
#define BODYPIX_PART_RIGHT_HAND             11
#define BODYPIX_PART_TORSO_FRONT            12
#define BODYPIX_PART_TORSO_BACK             13
#define BODYPIX_PART_LEFT_UPPER_LEG_FRONT   14
#define BODYPIX_PART_LEFT_UPPER_LEG_BACK    15
#define BODYPIX_PART_RIGHT_UPPER_LEG_FRONT  16
#define BODYPIX_PART_RIGHT_UPPER_LEG_BACK   17
#define BODYPIX_PART_LEFT_LOWER_LEG_FRONT   18
#define BODYPIX_PART_LEFT_LOWER_LEG_BACK    19
#define BODYPIX_PART_RIGHT_LOWER_LEG_FRONT  20
#define BODYPIX_PART_RIGHT_LOWER_LEG_BACK   21
#define BODYPIX_PART_LEFT_FEET              22
#define BODYPIX_PART_RIGHT_FEET             23
#define BODYPIX_PART_COUNT                  24

// Sigmoid function
float BodyPix_Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

//
// BodyPix mask data helpers
//

// Mask data structure
struct BodyPix_Mask
{
    // Four samples used for bilinear filtering
    float4 segm; // Segmentation
    float4 part; // Part

    // Sampling point offset
    float2 offs;
};

// Sampling function
BodyPix_Mask BodyPix_SampleMask(float2 uv, texture2D tex, uint2 tex_size)
{
    float2 coord = max(uv * tex_size - 0.5, 0);

    uint4 idx;
    idx.xy = coord;
    idx.zw = min(idx.xy + 1, tex_size - 1);

    float2 s00 = tex[idx.xy].xw;
    float2 s01 = tex[idx.zy].xw;
    float2 s10 = tex[idx.xw].xw;
    float2 s11 = tex[idx.zw].xw;

    BodyPix_Mask mask;
    mask.segm = float4(s00.y, s01.y, s10.y, s11.y);
    mask.part = float4(s00.x, s01.x, s10.x, s11.x);
    mask.offs = coord - idx.xy;
    return mask;
}

// Bilinear filtering function
float BodyPix_Bilinear(float4 s, float2 p)
{
    return lerp(lerp(s.x, s.y, p.x), lerp(s.z, s.w, p.x), p.y);
}

// Evaluate a segmentation value of sample data
float BodyPix_EvalSegmentation(const in BodyPix_Mask mask)
{
    return BodyPix_Bilinear(mask.segm, mask.offs);
}

// Evaludate a part value of sample data
float BodyPix_EvalPart(const in BodyPix_Mask mask, uint part_id)
{
    uint4 flags = (uint4)(mask.part * BODYPIX_PART_COUNT + 0.5) == part_id;
    return BodyPix_Bilinear(flags, mask.offs);
}

#endif
