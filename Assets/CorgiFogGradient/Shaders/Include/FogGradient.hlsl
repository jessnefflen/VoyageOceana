#ifndef CORGI_FOG_GRADIENT_DEFINED
#define CORGI_FOG_GRADIENT_DEFINED

Texture2D _FogGradientTexture;
SamplerState _FogGradientLinearClamp;

float4 _FogGradientData;

// https://www.chilliant.com/rgb2hsv.html
float3 RGBtoHCV(in float3 RGB)
{
    // Based on work by Sam Hocevar and Emil Persson
    const float Epsilon = 1e-10;
    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
    return float3(H, C, Q.x);
}

float3 HUEtoRGB(in float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R, G, B));
}

float3 RGBtoHSV(in float3 RGB)
{
    const float Epsilon = 1e-10;
    float3 HCV = RGBtoHCV(RGB);
    float S = HCV.y / (HCV.z + Epsilon);
    return float3(HCV.x, S, HCV.z);
}

float3 HSVtoRGB(in float3 HSV)
{
    float3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

float3 BlendHSVSpace(float3 a, float3 b, float t)
{
    float3 hsv_a = RGBtoHSV(a);
    float3 hsv_b = RGBtoHSV(b);

    float3 hsv_c = lerp(hsv_a, hsv_b, t);

    return HSVtoRGB(hsv_c);
}

float4 ApplyFogGradient(float3 color, float distance, float height)
{
    float sample_x = (distance - _FogGradientData.x) / (_FogGradientData.y - _FogGradientData.x);
    float sample_y = (height - _FogGradientData.z) / (_FogGradientData.w - _FogGradientData.z);
    float4 fogColor = _FogGradientTexture.SampleLevel(_FogGradientLinearClamp, saturate(float2(sample_x, sample_y)), 0);

#ifdef _FOG_GRADIENT_HSV_BLEND_ENABLED
    return float4(BlendHSVSpace(color.rgb, fogColor.rgb, fogColor.a), fogColor.a);
#else
    return float4(lerp(color.rgb, fogColor.rgb, fogColor.a), fogColor.a);
#endif
}

#ifdef _FOG_GRADIENT_ON
	#define FRAGMENT_FOGGRADIENT(color,  distance, height) color.rgb = ApplyFogGradient(color.rgb, distance, height).rgb;
#else
	#define FRAGMENT_FOGGRADIENT(color,  distance, height)
#endif

#endif