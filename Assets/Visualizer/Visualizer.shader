Shader "Hidden/BodyPix/Visualizer"
{
    Properties
    {
        _MainTex("", 2D) = "black" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Packages/jp.keijiro.bodypix/Shaders/Common.hlsl"

    float3 HueToRGB(float h)
    {
        h = frac(saturate(h)) * 6 - 2;
        float3 c = saturate(float3(abs(h - 1) - 1, 2 - abs(h), 2 - abs(h - 2)));
        #ifndef UNITY_COLORSPACE_GAMMA
        c = GammaToLinearSpace(c);
        #endif
        return c;
    }

    //
    // Mask
    //

    texture2D _MainTex;
    float4 _MainTex_TexelSize;

    void VertexMask(float4 position : POSITION,
                    float2 texCoord : TEXCOORD,
                    out float4 outPosition : SV_Position,
                    out float2 outTexCoord : TEXCOORD)
    {
        outPosition = UnityObjectToClipPos(position);
        outTexCoord = texCoord;
    }

    float4 FragmentMask(float4 position : SV_Position,
                        float2 texCoord : TEXCOORD) : SV_Target
    {
        BodyPix_Mask mask =
          BodyPix_SampleMask(texCoord, _MainTex, _MainTex_TexelSize.zw);

        float3 acc = 0;
        for (uint part = 0; part < BODYPIX_PART_COUNT; part++)
        {
            float score = BodyPix_EvalPart(mask, part);
            score = smoothstep(0.47, 0.57, score);
            acc += HueToRGB((float)part / BODYPIX_PART_COUNT) * score;
        }

        float alpha = BodyPix_EvalSegmentation(mask);
        alpha = smoothstep(0.47, 0.57, alpha);

        return float4(acc, alpha);
    }

    //
    // Keypoints
    //

    StructuredBuffer<float4> _Keypoints;
    float _Aspect;

    void VertexKeypoints(uint vid : SV_VertexID,
                         uint iid : SV_InstanceID,
                         out float4 position : SV_Position,
                         out float4 color : COLOR)
    {
        float4 key = _Keypoints[iid];

        const float threshold = 0.5;
        float alpha = saturate((key.z - threshold) / (1 - threshold));

        float x = lerp(-0.5, 0.5, key.x) * _Aspect;
        float y = lerp(-0.5, 0.5, key.y);

        float vx = lerp(-0.5, 0.5, vid & 1);
        float vy = lerp(-0.5, 0.5, vid < 2 || vid == 5);

        vx *= 0.015 * alpha;
        vy *= 0.015 * alpha;

        position = UnityObjectToClipPos(float4(x + vx, y + vy, 1, 1));
        color = float4(1, 1, 0, alpha);
    }

    float4 FragmentKeypoints(float4 position : SV_Position,
                             float4 color : COLOR) : SV_Target
    {
        return color;
    }

    //
    // Bones
    //

    static const uint bone_connections[12][2] =
    {
        { BODYPIX_KEYPOINT_LEFT_HIP,        BODYPIX_KEYPOINT_LEFT_SHOULDER  },
        { BODYPIX_KEYPOINT_LEFT_ELBOW,      BODYPIX_KEYPOINT_LEFT_SHOULDER  },
        { BODYPIX_KEYPOINT_LEFT_ELBOW,      BODYPIX_KEYPOINT_LEFT_WRIST     },
        { BODYPIX_KEYPOINT_LEFT_HIP,        BODYPIX_KEYPOINT_LEFT_KNEE      },
        { BODYPIX_KEYPOINT_LEFT_KNEE,       BODYPIX_KEYPOINT_LEFT_ANKLE     },

        { BODYPIX_KEYPOINT_RIGHT_HIP,       BODYPIX_KEYPOINT_RIGHT_SHOULDER },
        { BODYPIX_KEYPOINT_RIGHT_ELBOW,     BODYPIX_KEYPOINT_RIGHT_SHOULDER },
        { BODYPIX_KEYPOINT_RIGHT_ELBOW,     BODYPIX_KEYPOINT_RIGHT_WRIST    },
        { BODYPIX_KEYPOINT_RIGHT_HIP,       BODYPIX_KEYPOINT_RIGHT_KNEE     },
        { BODYPIX_KEYPOINT_RIGHT_KNEE,      BODYPIX_KEYPOINT_RIGHT_ANKLE    },

        { BODYPIX_KEYPOINT_LEFT_SHOULDER,   BODYPIX_KEYPOINT_RIGHT_SHOULDER },
        { BODYPIX_KEYPOINT_LEFT_HIP,        BODYPIX_KEYPOINT_RIGHT_HIP      }
    };

    void VertexBones(uint vid : SV_VertexID,
                     uint iid : SV_InstanceID,
                     out float4 position : SV_Position,
                     out float4 color : COLOR)
    {
        float4 key = _Keypoints[bone_connections[iid][vid]];

        float x = lerp(-0.5, 0.5, key.x) * _Aspect;
        float y = lerp(-0.5, 0.5, key.y);

        const float threshold = 0.3;
        bool mask = key.z > threshold;

        position = UnityObjectToClipPos(float4(x, y, 1, 1));
        color = float4(1, 1, 0, mask);
    }

    float4 FragmentBones(float4 position : SV_Position,
                         float4 color : COLOR) : SV_Target
    {
        clip(color.a - 1);
        return color;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex VertexMask
            #pragma fragment FragmentMask
            ENDCG
        }

        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex VertexKeypoints
            #pragma fragment FragmentKeypoints
            ENDCG
        }

        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex VertexBones
            #pragma fragment FragmentBones
            ENDCG
        }
    }
}
