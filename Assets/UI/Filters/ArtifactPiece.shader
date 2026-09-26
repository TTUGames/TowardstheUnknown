// UI Toolkit filter animating an artifact's piece in the inventory (ArtifactPiece) by its rarity, in the language of the
// relics on the floor: its surface drifts like their nebula, veins of the rarity's glow run through it from rare on, a
// sheen sweeps it (faint for rare, strong for legendary), legendary is gold (a soft gradient from a deep to a pale gold,
// star sparkles). The white icon pulses gently. Transparent pixels stay untouched.
// Parameters: the rarity's glow tone, the rarity plus 10 times a seed, the piece's width over its height, the time
Shader "Hidden/UI/ArtifactPiece"
{
    CGINCLUDE
    #include "UnityCG.cginc"
    #include "UnityUIEFilter.cginc"

    sampler2D _MainTex;
    float4 _Glow;
    float _RarityAndSeed;
    float _Aspect;
    float _PieceTime;

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

    float Hash(float3 p)
    {
        p = frac(p * 0.3183099 + 0.1);
        p *= 17.0;
        return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
    }

    float Hash1(float n) { return frac(sin(n * 127.1 + 311.7) * 43758.5453); }

    float ValueNoise(float3 p)
    {
        float3 i = floor(p);
        float3 f = frac(p);
        f = f * f * (3.0 - 2.0 * f);
        return lerp(lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x),
                         lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                    lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x),
                         lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
    }

    fixed4 frag(v2f i) : SV_Target
    {
        float rarity = fmod(_RarityAndSeed, 10);
        float seed = floor(_RarityAndSeed / 10);
        float time = _PieceTime;
        // Position in the piece, from 0 to 1, x scaled by the aspect so that the patterns stay round
        float2 p = (i.uv - i.rect.xy) / i.rect.zw;
        float2 q = float2(p.x * _Aspect, p.y);

        fixed4 color = tex2D(_MainTex, i.uv);
        if (color.a <= 0.001) return color;

        // The white icon, apart from the surface
        float icon = smoothstep(0.75, 0.95, min(color.r, min(color.g, color.b)));
        float3 surface = color.rgb;

        // The nebula drifting through the surface, livelier with the rarity
        float3 n = float3(q * 3, time * 0.12 + seed);
        float nebula = ValueNoise(n) * 0.65 + ValueNoise(n * 2.3 + 4.1) * 0.35;
        surface *= 0.82 + (0.25 + 0.08 * rarity) * nebula;

        // Veins of the glow, from rare on
        float veins = pow(saturate(1 - abs(ValueNoise(float3(q * 2.2, time * 0.18 + seed + 9)) - 0.5) * 7), 3);
        surface += _Glow.rgb * veins * 0.18 * step(0.5, rarity);

        // A sheen sweeping along the diagonal, every few seconds
        float sweep = frac(time * 0.22 + Hash1(seed));
        float d = (p.x + p.y * 0.45) / 1.45 - (sweep * 2.2 - 0.6);
        float sheen = exp(-d * d * 90);
        float sheenStrength = rarity < 0.5 ? 0 : rarity < 1.5 ? 0.12 : rarity < 2.5 ? 0.22 : 0.55;
        surface += lerp(float3(1, 1, 1), _Glow.rgb, 0.4) * sheen * sheenStrength;

        // Legendary is gold: a soft gradient from a deep to a pale gold, and star sparkles
        if (rarity > 2.5)
        {
            // From a deep amber to a pale, yellower gold, both drawn from the rarity's color
            float3 deep = color.rgb * float3(0.62, 0.5, 0.3);
            float3 pale = lerp(color.rgb * 1.25, float3(1, 0.88, 0.5), 0.45);
            float metal = saturate(p.y * 0.6 + nebula * 0.4);
            float3 gold = lerp(deep, pale, metal);
            gold += float3(1, 0.92, 0.65) * sheen * sheenStrength;
            surface = gold;

            float2 grid = q * 6;
            float2 cell = floor(grid);
            float2 f = frac(grid) - 0.5 + (float2(Hash(float3(cell, seed + 1)), Hash(float3(cell, seed + 2))) - 0.5) * 0.5;
            float star = exp(-length(f) * 18) + exp(-abs(f.x) * 60) * exp(-abs(f.y) * 9) + exp(-abs(f.y) * 60) * exp(-abs(f.x) * 9);
            float twinkle = step(0.8, Hash(float3(cell, seed))) * pow(saturate(sin(time * 2.2 + Hash(float3(cell, 7)) * 40)), 8);
            surface += float3(1, 0.93, 0.75) * star * twinkle * 0.9;
        }

        // The icon pulses gently, touched by the glow
        float pulse = 1 + 0.08 * sin(time * 2 + seed);
        float3 iconColor = lerp(color.rgb, color.rgb * lerp(float3(1, 1, 1), normalize(_Glow.rgb + 0.001) * 1.2, 0.15), step(0.5, rarity)) * pulse;

        color.rgb = lerp(surface, iconColor, icon);
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
