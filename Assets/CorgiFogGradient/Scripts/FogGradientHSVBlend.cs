using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace CorgiFogGradient
{
    public static class FogGradientHSVBlend 
    {
        // https://www.chilliant.com/rgb2hsv.html
        public static float3 RGBtoHCV(in float3 RGB)
        {
            // Based on work by Sam Hocevar and Emil Persson
            const float Epsilon = 1e-10f;
            float4 P = (RGB.y < RGB.z) ? new float4(RGB.zy, -1.0f, 2.0f / 3.0f) : new float4(RGB.yz, 0.0f, -1.0f / 3.0f);
            float4 Q = (RGB.x < P.x) ? new float4(P.xyw, RGB.x) : new float4(RGB.x, P.yzx);
            float C = Q.x - math.min(Q.w, Q.y);
            float H = math.abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
            return new float3(H, C, Q.x);
        }

        public static float3 HUEtoRGB(in float H)
        {
            float R = math.abs(H * 6 - 3) - 1;
            float G = 2 - math.abs(H * 6 - 2);
            float B = 2 - math.abs(H * 6 - 4);
            return math.saturate(new float3(R, G, B));
        }

        public static float3 RGBtoHSV(in float3 RGB)
        {
            const float Epsilon = 1e-10f;
            float3 HCV = RGBtoHCV(RGB);
            float S = HCV.y / (HCV.z + Epsilon);
            return new float3(HCV.x, S, HCV.z);
        }

        public static float3 HSVtoRGB(in float3 HSV)
        {
            float3 RGB = HUEtoRGB(HSV.x);
            return ((RGB - 1) * HSV.y + 1) * HSV.z;
        }

        public static float3 BlendHSVSpace(float3 a, float3 b, float t)
        {
            float3 hsv_a = RGBtoHSV(a);
            float3 hsv_b = RGBtoHSV(b);

            float3 hsv_c = math.lerp(hsv_a, hsv_b, t);

            return HSVtoRGB(hsv_c);
        }

        public static Color BlendHSVSpace(Color a, Color b, float t)
        {
            float3 hsv_a = RGBtoHSV(new float3(a.r, a.g, a.b));
            float3 hsv_b = RGBtoHSV(new float3(b.r, b.g, b.b));

            float3 hsv_c = math.lerp(hsv_a, hsv_b, t);
            float3 rgb = HSVtoRGB(hsv_c);

            return new Color(rgb.x, rgb.y, rgb.z, math.lerp(a.a, b.a, t));
        }
    }
}
