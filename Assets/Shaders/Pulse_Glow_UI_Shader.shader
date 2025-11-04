Shader "Custom/PulseGlowUI_NoTexture"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (1,0,0,1)  // Kırmızı glow
        _GlowRadius ("Glow Radius", Range(0,1)) = 0.5
        _PulseSpeed ("Pulse Speed", Range(0,10)) = 2
        _PulseMin ("Pulse Min Alpha", Range(0,1)) = 0.3
        _PulseMax ("Pulse Max Alpha", Range(0,1)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _GlowColor;
            float _GlowRadius;
            float _PulseSpeed;
            float _PulseMin;
            float _PulseMax;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _TimeY; // Unity'de _Time.y genellikle zamanı verir

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord; // Direct UV coords, no texture transform
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Merkezi (0.5,0.5) hesapla
                float2 center = float2(0.5, 0.5);
                
                // Aspect ratio hesapla: ekran boyutuna bağlı olarak UV'yi yeniden ölçeklendir
                float aspectRatio = _ScreenParams.x / _ScreenParams.y;
                float2 scaledUV = (i.uv - center) * float2(aspectRatio, 1.0) + center;

                // Glow mesafesini ölçeklenmiş UV ile hesapla
                float dist = distance(scaledUV, center);

                // Glow alpha'sı mesafe ile azalan değer
                float glowAlpha = smoothstep(_GlowRadius, 0.0, dist);

                // Köşe noktaları
                float2 corner1 = float2(0.0, 0.0);
                float2 corner2 = float2(1.0, 0.0);
                float2 corner3 = float2(0.0, 1.0);
                float2 corner4 = float2(1.0, 1.0);

                // Köşelere uzaklık hesapla
                float d1 = distance(i.uv, corner1);
                float d2 = distance(i.uv, corner2);
                float d3 = distance(i.uv, corner3);
                float d4 = distance(i.uv, corner4);

                // En yakın köşeye göre glow
                float cornerDist = min(min(d1, d2), min(d3, d4));
                float cornerGlow = smoothstep(_GlowRadius, 0.0, cornerDist);

                // Pulse animasyonu
                float pulse = lerp(_PulseMin, _PulseMax, (sin(_Time.y * _PulseSpeed) + 1.0) / 2.0);

                // Toplam glow (merkez + köşe)
                float totalGlowAlpha = max(glowAlpha, cornerGlow);

                fixed4 glow = _GlowColor * totalGlowAlpha * pulse;

                // Orijinal renk yok, sadece glow var
                glow.a = saturate(glow.a);
                return glow;
            }
            ENDCG
        }
    }
}
