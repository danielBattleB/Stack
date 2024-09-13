Shader "Unlit/GradientShader"
{
    Properties
    {
        _TopColor("Top Color", Color) = (1, 1, 1, 1) // White
        _BottomColor("Bottom Color", Color) = (0, 0, 0, 1) // Black
        _BrightnessFactor("Brightness Factor", Float) = 2 // Factor to increase brightness
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
            float _BrightnessFactor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy; // Using vertex position to calculate the gradient
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate the gradient factor based on the Y position, scaled from 0 to 1
                float gradient = saturate(i.uv.y * 0.5 + 0.5); // Ensuring the gradient is between 0 and 1

                // Brighter colors near the top and bottom
                float brightGradient = pow(gradient, 0.7); // Adjust the power value to control brightness near top
                float darkGradient = pow(1.0 - gradient, 0.7); // Adjust the power value to control brightness near bottom

                // Interpolating and adjusting brightness at both top and bottom
                fixed4 color = lerp(_BottomColor * _BrightnessFactor * darkGradient, _TopColor * _BrightnessFactor * brightGradient, gradient);

                // Clamp the final color to not exceed the maximum color range
                return saturate(color);
            }
            ENDCG
        }
    }
}
