// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Textured With Detail"
{
    Properties
    {
        _Tint("Tint", Color) = (1, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex MyVertex
            #pragma fragment MyFragment

            #include "UnityCG.cginc"

            float4 _Tint;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            struct Interpolators
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct VertexData
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            Interpolators MyVertex(VertexData v)
            {
                Interpolators i;
                i.position = UnityObjectToClipPos(v.position);
                i.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; // Adds tiling and offset. If you use a sprite instead of a texture, the image will resize and offset but not repeat
                //i.localPosition = i.position.xyz; // By having the local position used for texturing be based on the world adjusted position, the colours change when the object moves. Funky!
                return i;
                // return mul(UNITY_MATRIX_MVP, position);
            }

            float4 MyFragment(Interpolators i) : SV_TARGET
            {
                //return _Tint;
                float4 colour = tex2D(_MainTex, i.uv * 10) * _Tint;
                return colour;
            }

            /*
            Interpolators MyVertex(float4 position : POSITION, float2 uv : TEXCOORD0)
            {
                Interpolators i;
                i.localPosition = position.xyz;
                i.position = UnityObjectToClipPos(position);
                //i.localPosition = i.position.xyz; // By having the local position used for texturing be based on the world adjusted position, the colours change when the object moves. Funky!
                return i;
                // return mul(UNITY_MATRIX_MVP, position);
            }

            float4 MyFragment(Interpolators i) : SV_TARGET
            {
                //return _Tint;
                return float4(i.localPosition + 0.5, 1) * _Tint;
            }
            */

            ENDCG
        }
    }
}
