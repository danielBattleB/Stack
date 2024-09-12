Shader "Unlit/GradientShader"
{
    Properties
    {
        _TopColor("Top Color", Color) = (1, 1, 1, 1) // White
        _BottomColor("Bottom Color", Color) = (0, 0, 0, 1) // Black
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _TopColor;
            fixed4 _BottomColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy; // Using vertex position to calculate the gradient
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Linear interpolation between top and bottom color based on the y position
                float gradient = i.uv.y * 0.5 + 0.5; // Adjust range from 0 to 1
                return lerp(_BottomColor, _TopColor, gradient);
            }
            ENDCG
        }
    }
}
