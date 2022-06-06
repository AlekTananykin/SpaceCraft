Shader "Unlit/PerlinLanscape"
{
    Properties
    {
        _MountainColor("Mountain Color", COLOR) = (0.6, 0.3, 0.3, 1)
        _GrassColor("Grass Color", COLOR) = (0.1, 0.6, 0.3, 1)
        _WaterColor("Water Color", COLOR) = (0.1, 0.3, 0.5, 1)

        _Seed("Seed", Range(0, 10000)) = 10
    }
    SubShader
    {
        LOD 200

        Tags{"RenderType" = "Opaque"}

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex: SV_POSITION;
                float4 height: COLOR;
            };

            float4 _MountainColor;
            float4 _GrassColor;
            float4 _WaterColor;

            float _Seed;

            float hash(float2 st) 
            {
                return frac(sin(dot(st.xy
                    , float2(12.9898, 78.233))) 
                    * 43758.5453123);
            }

            float noise(float2 p, float size)
            {
                float result = 0;
                p *= size;
                float2 i = floor(p + _Seed);
                float2 f = frac(p + _Seed / 739);
                float2 e = float2(0, 1);
                float z0 = hash((i + e.xx) % size);
                float z1 = hash((i + e.yx) % size);
                float z2 = hash((i + e.xy) % size);
                float z3 = hash((i + e.yy) % size);
                float2 u = smoothstep(0, 1, f);
                result = lerp(z0, z1, u.x) + (z2 - z0) * u.y * (1.0 - u.x) + (z3 - z1) *
                u.x * u.y;
                return result;
            }

            v2f vert(appdata_full v)
            {
                v2f result;

                float height = noise(v.texcoord, 5) * 0.75
                    + noise(v.texcoord, 30) * 0.125 
                    + noise(v.texcoord, 50) * 0.125;

                result.height.r = height;

                result.vertex = 
                    UnityObjectToClipPos(v.vertex);

                return result;
            }

            fixed4 frag(v2f IN): SV_Target
            {
                fixed4 color = (0.1, 0.1, 0.1, 0.1);
                float height = IN.height.r;
                if (height < 0.45)
                {
                    color = _WaterColor;
                }
                else if (height < 0.75)
                {
                    color = _GrassColor;
                }
                else
                {
                    color = _MountainColor;
                }
                
                return color;
            }
            ENDCG
        }        
    }
}
