Shader "Hidden/CorgiFog/BlitShader"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Include/FogGradient.hlsl"

    #pragma target 5.0

    struct AttributesDefault
    {
        float4 positionHCS : POSITION;
        float2 uv          : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct VaryingsDefault
    {
        float4 positionCS  : SV_POSITION;
        float2 uv : TEXCOORD0;
        float4 positionHCS : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    VaryingsDefault VertDefault(AttributesDefault v)
    {
        VaryingsDefault o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        o.positionCS = float4(v.positionHCS.xyz, 1.0);
        o.positionHCS = v.positionHCS;

#if UNITY_UV_STARTS_AT_TOP
        o.positionCS.y *= -1;
#endif

        o.uv = v.uv;

        return o;
    }

    float4x4 corgi_CameraToWorld;
    float4x4 corgi_InverseProjection;

    float3 GetWorldSpacePosition(float depth, float2 uv)
    {
        float4 clip = float4(2.0 * uv - 1.0, depth, 1.0);
        float4 viewPos = mul(corgi_InverseProjection, clip);
        viewPos.xyz /= viewPos.w;
        viewPos.w = 1;

        float3 worldPos = mul(corgi_CameraToWorld, viewPos).xyz;
        return worldPos;
    }

    TEXTURE2D_X(_CorgiGrabpass);
    SAMPLER(sampler_CorgiGrabpass);

    #ifdef _FOG_GRADIENT_USE_CORGI_DEPTH_BLIT
        TEXTURE2D_X(_CorgiDepthGrabpass);
        SAMPLER(sampler_CorgiDepthGrabpass);
    #endif

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

        #ifdef _FOG_GRADIENT_USE_CORGI_DEPTH_BLIT
            float depth = SAMPLE_TEXTURE2D_X(_CorgiDepthGrabpass, sampler_CorgiDepthGrabpass, i.uv).r;
        #else
            float depth = SampleSceneDepth(i.uv);
        #endif

        float3 worldPosition = GetWorldSpacePosition(depth, i.uv);
        float distance = length(worldPosition - _WorldSpaceCameraPos); 
        float height = abs(worldPosition.y - _WorldSpaceCameraPos.y);

        float4 col = SAMPLE_TEXTURE2D_X(_CorgiGrabpass, sampler_CorgiGrabpass, i.uv);
        float4 result = ApplyFogGradient(col.rgb, distance, height);
        
        if (depth < 0.00001)
        {
            return col; 
        }

        return float4(result.rgb, 1); 
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Cull Off
        ZWrite Off
        ZTest Always

        // Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

                #pragma multi_compile_instancing
                #pragma multi_compile _ _FOG_GRADIENT_HSV_BLEND_ENABLED
                #pragma multi_compile _ _FOG_GRADIENT_USE_CORGI_DEPTH_BLIT

            ENDHLSL
        }
    }
}