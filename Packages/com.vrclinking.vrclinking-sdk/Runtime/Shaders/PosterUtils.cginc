#ifndef POSTER_UTILS_INCLUDED
#define POSTER_UTILS_INCLUDED

#include "PosterInput.cginc"

float2 ApplyAspect(float2 uv, float width, float height)
{
    uv -= 0.5;

    if (width < height)
    {
        uv.x *= height / width;
    }
    else
    {
        uv.y *= width / height;
    }

    uv += 0.5;

    return uv;
}

float2 ApplyAspect(float2 uv)
{
    return ApplyAspect(uv, GetContentWidth(), GetContentHeight());
}

float ComputeBoxMask(float2 uv, float width, float height)
{
    float axis = width < height ? uv.x : uv.y;
    float boundSdf = 0.5 - abs(axis - 0.5);
    return saturate(boundSdf / fwidth(axis));
}

float4 BoxContent(float2 uv, float width, float height, float4 contentColor, float4 boxColor)
{
    float4 col = contentColor;
    if (width != height)
    {
        col = lerp(boxColor, col, ComputeBoxMask(uv, width, height));
    }

    return col;
}

float4 BoxContent(float2 uv, float4 contentColor, float4 boxColor)
{
    return BoxContent(uv, GetContentWidth(), GetContentHeight(), contentColor, boxColor);
}

#endif