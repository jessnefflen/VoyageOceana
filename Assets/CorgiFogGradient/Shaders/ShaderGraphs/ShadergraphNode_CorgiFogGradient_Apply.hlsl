#include "../Include/FogGradient.hlsl"

void ApplyFogGradient_float(float3 color, float distance, float height, out float3 result)
{
	result = ApplyFogGradient(color, distance, height);
}   