Shader "Custom/GrayscaleShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Threshold ("Black Threshold", Range(0, 1)) = 0.2 // 控制黑色区域的阈值
        _GrayFactor ("Gray Factor", Range(0, 1)) = 1.0 // 控制灰度化的强度
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        // 设置渲染队列为 "Overlay" 确保透明图层在普通图层之后渲染
        Tags { "Queue"="Overlay" }

        Pass
        {
            // 使用透明度混合
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Threshold;      // 控制黑色区域的阈值
            float _GrayFactor;     // 控制灰度化的强度

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);  // 转换顶点位置到裁剪空间
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex); // 处理纹理坐标
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);

                if (col.a > 0.0)
                {
                    float gray = dot(col.rgb, half3(0.299, 0.587, 0.114));

                    if (gray < _Threshold)
                    {
                        col.rgb = gray.xxx * _GrayFactor;
                    }
                    else
                    {
                        col.rgb = gray.xxx * _GrayFactor;
                    }
                }

                return col;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}