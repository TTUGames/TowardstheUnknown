// UI Toolkit backdrop filter blurring what is behind an element except in its corners cut at 45°,
// so that the blur follows the slanted shapes drawn by CutShape.
// A separable gaussian blur: pass 0 is horizontal, pass 1 vertical. Both copy the cut corners unchanged
Shader "Hidden/UI/SlantedBlur"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    #include "UnityUIEFilter.cginc"

    // Samples on each side of the blurred pixel
    #define TAPS 16

    sampler2D _MainTex;
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

    // 1 when the pixel is in a cut corner of the element
    float InCutCorner(v2f i)
    {
        // Position in the element in points, from its top left corner
        float2 p = saturate((i.uv - i.rect.xy) / i.rect.zw);
        #if UNITY_UV_STARTS_AT_TOP
        p.y = 1 - p.y;
        #endif
        float2 size = float2(_Width, _Height);
        p *= size;
        float flags = floor(_CutAndCorners / 4096);
        float cut = _CutAndCorners - flags * 4096;
        return saturate(HasCorner(flags, 1) * IsCut(p, cut)
                      + HasCorner(flags, 2) * IsCut(float2(size.x - p.x, p.y), cut)
                      + HasCorner(flags, 4) * IsCut(size - p, cut)
                      + HasCorner(flags, 8) * IsCut(float2(p.x, size.y - p.y), cut));
    }

    // Gaussian blur along one axis, the radius being given in uv
    fixed4 Blur(v2f i, float2 radius)
    {
        fixed4 sum = 0;
        float weights = 0;
        for (int t = -TAPS; t <= TAPS; t++)
        {
            float x = (float)t / TAPS;
            float weight = exp(-2 * x * x);
            sum += tex2D(_MainTex, i.uv + radius * x) * weight;
            weights += weight;
        }
        return sum / weights;
    }

    fixed4 BlurAxis(v2f i, float2 axis)
    {
        if (InCutCorner(i) > 0)
            return tex2D(_MainTex, i.uv);
        return Blur(i, axis * _Radius * i.rect.zw / float2(_Width, _Height));
    }

    fixed4 horizontal(v2f i) : SV_Target { return BlurAxis(i, float2(1, 0)); }
    fixed4 vertical(v2f i) : SV_Target { return BlurAxis(i, float2(0, 1)); }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment horizontal
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment vertical
            ENDCG
        }
    }
}
