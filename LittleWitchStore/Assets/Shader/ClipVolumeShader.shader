Shader "Custom/VolumeClippingShader"
{
    Properties
    {
        _MainTex ("主纹理", 2D) = "white" {}
        _Color   ("整体颜色", Color) = (1,1,1,1)
        // 半尺寸放在属性里无妨，也能在脚本里全局设置
        _HalfSize ("CV 半尺寸", Vector) = (0.5,0.5,0.5,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #pragma target 3.0

            struct Attributes { float4 positionOS : POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings   { float4 positionHCS:SV_POSITION;  float3 cvPosLS:TEXCOORD0; float2 uv:TEXCOORD1; float3 normalWS:TEXCOORD2; };

            // ← 直接声明，不放到 Properties
            float4x4 _CV_WorldToLocal;
            float3   _HalfSize;          // 由脚本给 Shader.SetGlobalVector
            float4   _Color;
            sampler2D _MainTex; float4 _MainTex_ST;

            Varyings vert (Attributes v)
            {
                Varyings o;
                float4 worldPos = mul(unity_ObjectToWorld, v.positionOS);
                o.cvPosLS       = mul(_CV_WorldToLocal, worldPos).xyz;
                o.positionHCS   = TransformWorldToHClip(worldPos);
                o.uv            = TRANSFORM_TEX(v.uv, _MainTex);
                o.normalWS      = normalize(mul((float3x3)unity_ObjectToWorld, v.normalOS));
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                // 体积裁剪
                if (abs(i.cvPosLS.x) > _HalfSize.x ||
                    abs(i.cvPosLS.y) > _HalfSize.y ||
                    abs(i.cvPosLS.z) > _HalfSize.z) clip(-1);

                float4 tex   = tex2D(_MainTex, i.uv) * _Color;
                Light  l     = GetMainLight();
                float  diff  = max(0, dot(i.normalWS, l.direction));
                float3 rgb   = tex.rgb * (0.2 + diff * l.color);
                return float4(rgb, tex.a);
            }
            ENDHLSL
        }
    }
}
