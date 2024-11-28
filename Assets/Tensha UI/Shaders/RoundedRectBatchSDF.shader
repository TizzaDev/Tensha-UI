Shader "TenshaUI/RoundedRectBatchSDF"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 dataID : TEXCOORD1;
                float2 texcoord : TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 dataID : TEXCOORD1;
                float2 texcoord : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _FillColor[512];
            fixed4 _OutlineColor[512];
            float _OutlineWidth[512];
            float2 _SDFSize[512];
            float4 _SDFRadii[512];
            float2 _SDFPadding[512];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.dataID = v.dataID;
                o.texcoord = v.texcoord;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float sdRoundedBox(float2 p, float2 b, float4 radii)
            {
                float4 r = radii;
                r.xy = (p.x > 0.0) ? r.xy : r.zw;
                r.x = (p.y > 0.0) ? r.x : r.y;
                float2 q = abs(p) - b + r.x;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                int dataID = (int)floor(IN.dataID);

                float2 _Size = _SDFSize[dataID];
                float4 _Radii = _SDFRadii[dataID];
                float2 _Padding = _SDFPadding[dataID];

                float2 normalizedPadding = float2(_Padding.x / _Size.x, _Padding.y / _Size.y);
                uv = uv * (1 + normalizedPadding * 2) - normalizedPadding * 2;

                fixed4 this_OutlineColor = _OutlineColor[dataID];
                float this_OutlineWidth = _OutlineWidth[dataID];
                fixed4 this_FillColor = _FillColor[dataID];

                float2 position = (uv - 0.5) * _Size;
                float2 halfSize = _Size * 0.5;

                // Signed distance field calculation
                float dist = sdRoundedBox(position, halfSize, _Radii);
                float delta = fwidth(dist);

                float fillAlpha = 1 - smoothstep(-delta, 0, dist);
                float outlineAlpha = (1 - smoothstep(this_OutlineWidth - delta, this_OutlineWidth, dist));

                outlineAlpha *= this_OutlineColor.a;
                fillAlpha *= this_FillColor.a;

                half4 effects = lerp(
                    half4(this_OutlineColor.rgb, outlineAlpha),
                    half4(this_FillColor.rgb, fillAlpha),
                    fillAlpha
                );

                //fixed4 texCol = tex2D(_MainTex, IN.uv);
                //Unity Stuff
                //UNITY_APPLY_FOG(IN.fogCoord, col);

                return effects;
            }
            ENDCG
        }
    }
}
