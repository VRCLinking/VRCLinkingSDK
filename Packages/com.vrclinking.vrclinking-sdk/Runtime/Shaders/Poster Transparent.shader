Shader "VRCLinking/Poster Transparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _NextTex ("Next Atlas", 2D) = "white" {}
        [HideInInspector] _NextRect ("Next Rect", Vector) = (1,1,0,0)
        [HideInInspector] _PosterAspects ("Content / Frame Aspects / Fit", Vector) = (1,1,1,0)
        [HideInInspector] _PosterState ("Progress / Mode / Direction / Enabled", Vector) = (0,0,0,0)
        [HideInInspector] _PosterAvailable ("Available Images", Vector) = (1,1,0,0)
        _FadeColor ("Fade Color", Color) = (0,0,0,1)
        _Color ("Color", Color) = (1, 1, 1, 1)

        [Header(Boxing)] [Space]
        [Toggle] _AspectCorrection ("Aspect Correction", Float) = 0
        _BoxingColor ("Boxing Color", Color) = (0, 0, 0, 1)
        _SurfaceDimensions ("Surface Dimensions", Vector) = (1, 1, 0, 0)
        _MainTex_Offset ("Texture Offset", Vector) = (1, 1, 0, 0)

        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull", Int) = 2
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "Base"

            Cull [_CullMode]
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM

            #pragma target 3.0
            #pragma vertex PosterVert
            #pragma fragment PosterFrag

            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Poster.cginc"

            ENDCG
        }

        Pass
        {
            Name "ShadowCaster"

            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            CGPROGRAM

            #define ALPHABLEND

            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile_shadowcaster

            #include "PosterShadow.cginc"

            ENDCG
        }
    }
}
