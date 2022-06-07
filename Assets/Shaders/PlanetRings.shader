Shader "Unlit/PlanetRings"
{
    Properties
    {
        _RingsColor("Rings Color", COLOR) = (0.1, 0.1, 0.1, 0.1)
        _InnerRadius("Inner Radius", float) = 0.08
        _OuterRadius("Outer Radius", float) = 0.125
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _RingsColor;
            float _InnerRadius;
            float _OuterRadius;

            struct v2f
            {
                float4 vertex : POSITION0;
                float4 uv: TEXCOORD0;
            };


            v2f vert (appdata_full v)
            {
                v2f o;
                o.uv = v.texcoord;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = (0.5, 0.5);
                float radius = distance(center, i.uv);

                if (radius < _InnerRadius || radius > _OuterRadius)
                    discard;

                fixed4 color = _RingsColor;
                return color;
            }
            ENDCG
        }
    }
}
