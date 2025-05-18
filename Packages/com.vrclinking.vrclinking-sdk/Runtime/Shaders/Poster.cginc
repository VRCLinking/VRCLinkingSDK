#ifndef POSTER_INCLUDED
#define POSTER_INCLUDED

#include "UnityCG.cginc"
#include "PosterInput.cginc"
#include "PosterUtils.cginc"

struct PosterAppdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct PosterVaryings
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


PosterVaryings PosterVert(PosterAppdata v)
{
    PosterVaryings o;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_OUTPUT(PosterVaryings, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = _AspectCorrection ? ApplyAspect(v.uv) : v.uv;

    UNITY_TRANSFER_FOG(o, o.vertex);
    return o;
}

fixed4 PosterFrag(PosterVaryings i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);

    float2 uv = i.uv;

    // Computing the tex uvs here let us get away with replacing a div with a mul whilst still only using 1 register.
    float4 scaleTranslation = GetTextureScaleTranslation();
    float2 texUv = uv * scaleTranslation.xy + scaleTranslation.zw;
    fixed4 col = UNITY_SAMPLE_TEX2D(_MainTex, texUv);

    if (_AspectCorrection)
    {
        col = BoxContent(uv, col, _BoxingColor);
    }

    col *= _Color;

    UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}

#endif