Shader "Hidden/BodyPix/Bones"
{
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Packages/jp.keijiro.bodypix/Shader/Common.hlsl"

    StructuredBuffer<float4> _Keypoints;
    float _Aspect;

    static const uint keypoint_connections[12][2] =
    {
        { PART_LEFT_HIP, PART_LEFT_SHOULDER },
        { PART_LEFT_ELBOW, PART_LEFT_SHOULDER },
        { PART_LEFT_ELBOW, PART_LEFT_WRIST },
        { PART_LEFT_HIP, PART_LEFT_KNEE },
        { PART_LEFT_KNEE, PART_LEFT_ANKLE },

        { PART_RIGHT_HIP, PART_RIGHT_SHOULDER },
        { PART_RIGHT_ELBOW, PART_RIGHT_SHOULDER },
        { PART_RIGHT_ELBOW, PART_RIGHT_WRIST },
        { PART_RIGHT_HIP, PART_RIGHT_KNEE },
        { PART_RIGHT_KNEE, PART_RIGHT_ANKLE },

        { PART_LEFT_SHOULDER, PART_RIGHT_SHOULDER },
        { PART_LEFT_HIP, PART_RIGHT_HIP }
    };

    void Vertex(uint vid : SV_VertexID,
                uint iid : SV_InstanceID,
                out float4 position : SV_Position,
                out float4 color : COLOR)
    {
        float4 key = _Keypoints[keypoint_connections[iid][vid]];

        float x = lerp(-1, 1, key.x) / _Aspect;
        float y = lerp(-1, 1, key.y);

        const float threshold = 0.5;
        bool mask = key.z > threshold;

        position = float4(x, y, 1, 1);
        color = float4(1, 1, 0, mask);
    }

    float4 Fragment(float4 position : SV_Position,
                    float4 color : COLOR) : SV_Target
    {
        clip(color.a - 1);
        return color;
    }

    ENDCG

    SubShader
    {
        Tags { "Queue"="Overlay+100" }
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
