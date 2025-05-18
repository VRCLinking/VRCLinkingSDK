#ifndef POSTER_SHADOW_INCLUDED
#define POSTER_SHADOW_INCLUDED

#include "UnityCG.cginc"
#include "Poster.cginc"

sampler3D _DitherMaskLOD;

struct ShadowVaryings
{
    V2F_SHADOW_CASTER;

    float2 uv : TEXCOORD0;

    UNITY_VERTEX_OUTPUT_STEREO
};

ShadowVaryings ShadowVert(appdata_base v)
{
    ShadowVaryings o;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.uv = _AspectCorrection ? ApplyAspect(v.texcoord, GetContentWidth(), GetContentHeight()) : v.texcoord;
    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

    return o;
}

float4 ShadowFrag(ShadowVaryings i) : SV_Target
{
    float2 uv = i.uv;

    // Computing the tex uvs here let us get away with replacing a div with a mul whilst still only using 1 register.
    float4 scaleTranslation = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_Offset);
    float2 texUv = uv * scaleTranslation.xy + scaleTranslation.zw;
    fixed4 col = UNITY_SAMPLE_TEX2D(_MainTex, texUv);

    if (_AspectCorrection)
    {
        col = BoxContent(uv, GetContentWidth(), GetContentHeight(), col, _BoxingColor);
    }

    col *= _Color;

    #ifdef ALPHABLEND
        // Use dither mask for alpha blended shadows, based on pixel position xy
        // and alpha level. Our dither texture is 4x4x16.
        half alphaRef = tex3D(_DitherMaskLOD, float3(i.pos.xy * 0.25, col.a * 0.9375)).a;
        clip(alphaRef - 0.01);
    #endif

    SHADOW_CASTER_FRAGMENT(i)
}

#endif