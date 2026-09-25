// UI Toolkit backdrop filter blurring what is behind an element except in its corners cut at 45°,
// so that the blur follows the slanted shapes drawn by CutShape
Shader "Hidden/UI/SlantedBlur"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUIEFilter.cginc"

            #define SAMPLES 48

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            // In points: the blur radius and the element's size
            float _Radius;
            float _Width;
            float _Height;
            // The cut size in points, plus 4096 times the flags of the cut corners
            // (1 top left, 2 top right, 4 bottom right, 8 bottom left): a filter function has 4 parameters at most
            float _CutAndCorners;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 rect : TEXCOORD1;
            };

            v2f vert(FilterVertexInput v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.rect = GetFilterUVRect(GetFilterRectIndex(v));
                return o;
            }

            // 1 when p (in points from the corner) is in the triangle cut from that corner
            float IsCut(float2 p, float cut)
            {
                return step(p.x + p.y, cut);
            }

            // 1 when the corner flag is set
            float HasCorner(float flags, float flag)
            {
                return step(flag, fmod(flags, flag * 2));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Position in the element in points, from its top left corner
                float2 p = saturate((i.uv - i.rect.xy) / i.rect.zw);
                #if UNITY_UV_STARTS_AT_TOP
                p.y = 1 - p.y;
                #endif
                float2 size = float2(_Width, _Height);
                p *= size;
                float flags = floor(_CutAndCorners / 4096);
                float c = _CutAndCorners - flags * 4096;
                float cut = HasCorner(flags, 1) * IsCut(p, c)
                          + HasCorner(flags, 2) * IsCut(float2(size.x - p.x, p.y), c)
                          + HasCorner(flags, 4) * IsCut(size - p, c)
                          + HasCorner(flags, 8) * IsCut(float2(p.x, size.y - p.y), c);
                if (cut > 0)
                    return tex2D(_MainTex, i.uv);

                // Samples spread on a disc along the golden angle, weighted by a gaussian
                float2 radius = _Radius * i.rect.zw / size;
                fixed4 sum = 0;
                float weights = 0;
                for (int s = 0; s < SAMPLES; s++)
                {
                    float r = sqrt((s + 0.5) / SAMPLES);
                    float angle = s * 2.39996323;
                    float weight = exp(-2 * r * r);
                    sum += tex2D(_MainTex, i.uv + float2(cos(angle), sin(angle)) * r * radius) * weight;
                    weights += weight;
                }
                return sum / weights;
            }
            ENDCG
        }
    }
}
