Shader "Custom/WhiteReplace"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_BaseColor("BaseColor", Color) = (1,1,1,1)
		_TargetColor("TargetColor", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
		Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
            };

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _BaseColor;
			fixed4 _TargetColor;
			
			v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }
			
			fixed4 frag (v2f IN) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
				if (col.r == _BaseColor.r && col.g == _BaseColor.g && col.b == _BaseColor.b) {
					col.rgb = _TargetColor.rgb;
				}
				col.rgb *= col.a;
				return col;
			}
			ENDCG
		}
	}
	Fallback "Transparent/Cutout/Diffuse"
}
