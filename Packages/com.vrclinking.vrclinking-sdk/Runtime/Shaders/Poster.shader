Shader "VRCLinking/Poster"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        [Header(Boxing)] [Space]
        [Toggle] _AspectCorrection ("Aspect Correction", Float) = 0
        _BoxingColor ("Boxing Color", Color) = (0, 0, 0)
        _SurfaceDimensions ("Surface Dimensions", Vector) = (1, 1, 0, 0)
        _MainTex_Offset ("Texture Offset", Vector) = (1, 1, 0, 0)

        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull", Int) = 2
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "Base"

            Cull [_CullMode]

            CGPROGRAM

            #pragma target 4.0
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

            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile_shadowcaster

            #include "PosterShadow.cginc"

            ENDCG
        }
    }
}
