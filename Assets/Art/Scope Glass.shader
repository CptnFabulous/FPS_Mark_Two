Shader "Custom/Scope Glass"
{
    Properties
    {
        _MainTex ("Render View", 2D) = "white" {}
        _Mask ("Shape Mask", 2D) = "white" {}
        _AlphaCutoff("Mask Alpha Cutoff", Range(0, 1)) = 0.5
        _Tint("Tint", Color) = (1,1,1,1)

    }
    SubShader
    {
        //Cull Off // If on, back-facing planes aren't rendered
        ZWrite Off // If on, ensures faces are rendered in the correct order based on distance from the camera
        //ZTest Always // Affects stuff in a similar way to ZWrite. But ZWrite is enabled because it eliminates the flicker on the reticle image
        
        Pass
        {
            CGPROGRAM


            #pragma vertex VertexFunction
            #pragma fragment FragmentFunction

            #include "UnityCG.cginc"

            float4 _Tint;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Mask;
            float4 _Mask_ST;
            float _AlphaCutoff;

            // So from what I can gather, SV_POSITION and SV_TARGET are the final 

            struct Interpolators
            {
                float4 position : SV_POSITION;
                float2 tex_uv : TEXCOORD0;
                float2 mask_uv : TEXCOORD0;
            };

            Interpolators VertexFunction(float4 position : POSITION, float2 tex_uv : TEXCOORD0, float2 mask_uv : TEXCOORD0)
            {
                Interpolators i;
                i.position = UnityObjectToClipPos(position); // Converts the coordinate value from screen to world position, to appropriately position the vertex. Otherwise the object will be stuck on a particular part of the screen.
                i.tex_uv = TRANSFORM_TEX(tex_uv, _MainTex);
                i.mask_uv = TRANSFORM_TEX(mask_uv, _Mask);
                return i;
            }

            float4 FragmentFunction(Interpolators i) : SV_TARGET
            {
                float4 colour = tex2D(_MainTex, i.tex_uv) * _Tint;
                float alpha = tex2D(_Mask, i.mask_uv).a;
                clip(alpha - _AlphaCutoff);
                return colour;
            }


            ENDCG
        }
    }
    FallBack "Diffuse"
}
