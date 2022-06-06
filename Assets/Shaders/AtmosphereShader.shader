Shader "Unlit/AtmosphereShader"
{
    Properties
    {
        _Color("Color", COLOR) = (0.6, 0.81, 0.92, 0.5)
        _Height("Height", Range(1.0, 2.0)) = 1.1
        _Transparency("Transparency", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float _Height;
            float _Transparency;

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata_full v)
            {
                v2f result;
                result.vertex = v.vertex * _Height;
                result.vertex = UnityObjectToClipPos(result.vertex);
                
                return result;
            }

            fixed4 frag(v2f v): SV_Target
            {
                fixed4 color = _Color;
                color.a = _Transparency;

                return color;
            }

            ENDCG
        }
    }
}
