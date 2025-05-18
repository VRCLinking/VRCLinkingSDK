#ifndef POSTER_INPUT_INCLUDED
#define POSTER_INPUT_INCLUDED

#include "UnityCG.cginc"

UNITY_DECLARE_TEX2D(_MainTex);
float4 _MainTex_TexelSize;
float4 _Color;

bool _AspectCorrection;
float4 _BoxingColor;

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _SurfaceDimensions)
UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_Offset)
UNITY_INSTANCING_BUFFER_END(Props)

float4 GetTextureScaleTranslation()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_Offset);
}

float4 GetSurfaceDimensions()
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _SurfaceDimensions);
}

float GetContentWidth()
{
    return GetTextureScaleTranslation().x * _MainTex_TexelSize.z * GetSurfaceDimensions().y;
}

float GetContentHeight()
{
    return GetTextureScaleTranslation().y * _MainTex_TexelSize.w * GetSurfaceDimensions().x;
}

#endif