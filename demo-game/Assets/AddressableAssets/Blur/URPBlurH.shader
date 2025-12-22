Shader "Custom/Blur/URPBlurH"
{
    Properties
    {
        _MainTex("Source Texture", 2D) = "white" {}
        _BlurSize("Blur Size", Range(0, 20)) = 3.0
        _DownSample("Downsample Factor", Range(1, 4)) = 2.0 // 降低解析度可以更模糊且更高效
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "HorizontalBlur"
            Blend One Zero
            ZWrite Off
            Cull Off
            ZTest Always

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 vertex : POSITION; 
                float2 uv     : TEXCOORD0;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                half2  uv  : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            half4 _MainTex_TexelSize; 
            half  _BlurSize;
            half  _DownSample;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.pos = TransformObjectToHClip(IN.vertex.xyz);
                OUT.uv  = half2(IN.uv);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half2 uv = IN.uv;

                half2 texelSize = _MainTex_TexelSize.xy * _DownSample;

                half weight[5];
                weight[0] = half(0.2270270);
                weight[1] = half(0.1945946);
                weight[2] = half(0.1216216);
                weight[3] = half(0.0540541);
                weight[4] = half(0.0162162);

                half4 color = _MainTex.Sample(sampler_MainTex, uv) * weight[0];

                [unroll]
                for (int i = 1; i < 5; i++)
                {
                    half offset = half(i) * _BlurSize;
                    color += _MainTex.Sample(sampler_MainTex, uv + half2(+offset * texelSize.x, 0)) * weight[i];
                    color += _MainTex.Sample(sampler_MainTex, uv + half2(-offset * texelSize.x, 0)) * weight[i];
                }

                return half4(color.rgb, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
