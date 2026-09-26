// UI Toolkit filter animating the parts of a health bar (HealthBar), lightly. The health: a soft vertical gradient, a
// faint current flowing along it and a sheen sweeping it every few seconds. The armor: an energy field, thin diagonal
// stripes drifting, a highlight along its top and a livelier sheen. Only the shape changes: the pixels outside it (the
// nearly transparent rectangle giving the image its bounds) stay untouched, and the colors are premultiplied by the alpha,
// so that nothing lights up past the shape's edges.
// Parameters: the mode (0 health, 1 armor), the part's width and height in points, the time
Shader "Hidden/UI/HealthBar"
{
    CGINCLUDE
    #include "UnityCG.cginc"
    #include "UnityUIEFilter.cginc"

    sampler2D _MainTex;
    float _BarMode;
    float _BarWidth;
    float _BarHeight;
    float _BarTime;

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

    fixed4 frag(v2f i) : SV_Target
    {
        fixed4 color = tex2D(_MainTex, i.uv);
        // Outside the shape: the bounds' rectangle is drawn at an alpha of 1/255
        if (color.a < 0.05) return color;
        // Position in the part, from 0 to 1, y going down; and in points
        float2 p = (i.uv - i.rect.xy) / i.rect.zw;
        float2 points = p * float2(_BarWidth, _BarHeight);
        float time = _BarTime;
        float3 rgb = color.rgb / color.a;

        // Lighter at the top, a little darker at the bottom
        rgb *= lerp(1.14, 0.9, p.y);

        // A sheen along the diagonal, sweeping in points so that it keeps its speed on a short bar
        float period = _BarMode < 0.5 ? 4.5 : 2.8;
        float travel = _BarWidth + 160;
        float sweepX = frac(time / period) * travel - 80;
        float d = (points.x + points.y * 0.6) - sweepX;
        float sheen = exp(-d * d / (_BarMode < 0.5 ? 900 : 500));

        if (_BarMode < 0.5)
        {
            // A faint current flowing along the bar
            float current = sin(points.x * 0.09 - time * 1.6 + sin(points.x * 0.023 + time * 0.7) * 2) * 0.5 + 0.5;
            rgb *= 0.96 + current * 0.06;
            rgb += sheen * 0.16;
        }
        else
        {
            // Thin diagonal stripes drifting, a highlight along the top
            float stripes = sin((points.x - points.y) * 0.45 - time * 3) * 0.5 + 0.5;
            rgb *= 0.93 + stripes * 0.12;
            rgb += exp(-pow((p.y - 0.18) * 9, 2)) * 0.12;
            rgb += sheen * 0.28;
        }

        color.rgb = saturate(rgb) * color.a;
        return color;
    }
    ENDCG

    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
