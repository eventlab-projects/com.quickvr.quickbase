#ifndef QUICK_MIRROR_CLIP_PLANE_DEFINITIONS_INC
#define QUICK_MIRROR_CLIP_PLANE_DEFINITIONS_INC

//Clipping plane definition in WS
float3 MIRROR_PLANE_POS;
float3 MIRROR_PLANE_NORMAL;

void clipPlaneTest(float3 wPos) 
{
	float3 v = wPos - MIRROR_PLANE_POS;
	clip(dot(v, MIRROR_PLANE_NORMAL));
}

#endif