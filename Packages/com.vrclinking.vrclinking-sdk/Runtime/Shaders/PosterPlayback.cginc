#ifndef POSTER_PLAYBACK_INCLUDED
#define POSTER_PLAYBACK_INCLUDED
#include "PosterInput.cginc"

float InsidePoster(float2 uv)
{
    return step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);
}

float2 PosterFitUv(float2 uv, float contentAspect)
{
    float ratio = max(contentAspect, 0.0001) / max(_PosterAspects.z, 0.0001);
    float2 scale = 1;
    if (_PosterAspects.w < 0.5) // Fit: pad with the background color.
        scale = ratio > 1 ? float2(1, ratio) : float2(1 / ratio, 1);
    else if (_PosterAspects.w < 1.5) // Fill: crop around the center.
        scale = ratio > 1 ? float2(1 / ratio, 1) : float2(1, ratio);
    return (uv - 0.5) * scale + 0.5;
}

float2 PosterAtlasUv(float2 uv, float4 rect, float4 texelSize)
{
    // Clamp inside this slot, including for fit/slide, to avoid adjacent atlas images bleeding in.
    float2 inset = min(abs(texelSize.xy) * 0.5, rect.xy * 0.5);
    return clamp(saturate(uv) * rect.xy + rect.zw, rect.zw + inset, rect.zw + rect.xy - inset);
}

fixed4 SamplePosterA(float2 uv)
{
    float2 fitted = PosterFitUv(uv, _PosterAspects.x);
    fixed4 color = UNITY_SAMPLE_TEX2D(_MainTex, PosterAtlasUv(fitted, GetTextureScaleTranslation(), _MainTex_TexelSize));
    return lerp(_BoxingColor, color, InsidePoster(fitted) * InsidePoster(uv) * _PosterAvailable.x);
}

fixed4 SamplePosterB(float2 uv)
{
    float2 fitted = PosterFitUv(uv, _PosterAspects.y);
    fixed4 color = UNITY_SAMPLE_TEX2D(_NextTex, PosterAtlasUv(fitted, _NextRect, _NextTex_TexelSize));
    return lerp(_BoxingColor, color, InsidePoster(fitted) * InsidePoster(uv) * _PosterAvailable.y);
}

fixed4 SamplePosterPlayback(float2 uv)
{
    float t = saturate(_PosterState.x);
    // The idle path samples only one atlas. Crossfades remain opaque in the opaque shader.
    UNITY_BRANCH if (t <= 0) return SamplePosterA(uv);
    UNITY_BRANCH if (t >= 1) return SamplePosterB(uv);
    float mode = _PosterState.y;
    if (mode < 0.5) return SamplePosterB(uv);
    if (mode < 1.5) return lerp(SamplePosterA(uv), SamplePosterB(uv), t);
    if (mode < 2.5)
        return t < 0.5 ? lerp(SamplePosterA(uv), _FadeColor, t * 2) : lerp(_FadeColor, SamplePosterB(uv), t * 2 - 1);
    float direction = _PosterState.z;
    float2 axis = direction < 0.5 ? float2(1, 0) : direction < 1.5 ? float2(-1, 0) : direction < 2.5 ? float2(0, -1) : float2(0, 1);
    if (mode < 3.5)
    {
        float2 nextUv = uv - axis * (1 - t);
        return lerp(SamplePosterA(uv + axis * t), SamplePosterB(nextUv), InsidePoster(nextUv));
    }
    float edge = dot(uv - 0.5, axis) + 0.5;
    return edge >= 1 - t ? SamplePosterB(uv) : SamplePosterA(uv);
}
#endif
