Shader "Hidden/BodyPix/Keypoint"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    StructuredBuffer<float4> _Keypoints;
    float _Aspect;

    void Vertex(uint vid : SV_VertexID,
                uint iid : SV_InstanceID,
                out float4 position : SV_Position,
                out float4 color : COLOR)
    {
        float4 key = _Keypoints[iid];

        float x = lerp(-1, 1, key.x) / _Aspect;
        float y = lerp(-1, 1, key.y);

        float vx = lerp(-1, 1, vid & 1);
        float vy = lerp(-1, 1, vid < 2 || vid == 5);

        vx *= 0.01 * _ScreenParams.y / _ScreenParams.x;
        vy *= 0.01;

        const float threshold = 0.5;
        float alpha = saturate((key.z - threshold) / (1 - threshold));

        position = float4(x + vx, y + vy, 1, 1);
        color = float4(1, 1, 0, alpha);
    }

    float4 Fragment(float4 position : SV_Position,
                    float4 color : COLOR) : SV_Target
    {
        return color;
    }

    ENDCG

    SubShader
    {
        Tags { "Queue"="Overlay+100" }
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
