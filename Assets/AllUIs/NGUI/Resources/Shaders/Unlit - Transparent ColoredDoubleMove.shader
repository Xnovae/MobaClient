// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Transparent ColoredDoubleMove"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
		_LightTex ("Base (RGB), Alpha (A)", 2D) = "black" {}

		_UVAnimX ("UV Anim X", float) = 0
		_UVAnimY ("UV Anim Y", float) = 0
		_Scale ("Scale", float) = 2
		_Mul ("Mul", float) = 1

		_UVAnimX1 ("UV Anim X1", float) = 0
		_UVAnimY1 ("UV Anim Y1", float) = 0
	}
	
	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _LightTex;

			uniform fixed _UVAnimX;
	        uniform fixed _UVAnimY;

	        uniform fixed _Scale;
	        uniform fixed _Mul;

			uniform fixed _UVAnimX1;
	        uniform fixed _UVAnimY1;
	
			struct appdata_t
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};
	
			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
				fixed2 uv1 : TEXCOORD1;
				fixed2 uv2 : TEXCOORD2;
			};
	
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;

				float t1 = sin(_Time.y*_UVAnimX);
				float t2 = sin(_Time.y*_UVAnimY);
				o.uv1 = (v.texcoord+ fixed2(t1, t2)*_Scale);

				float t3 = sin(_Time.y*_UVAnimX1);
				float t4 = sin(_Time.y*_UVAnimY1);
				o.uv2 = (v.texcoord+fixed2(t3, t4));
				return o;
			}
				
			fixed4 frag (v2f IN) : COLOR
			{
				fixed4 col = tex2D(_MainTex, IN.uv2) * IN.color;
				col.rgb += tex2D(_LightTex, IN.uv1).rgb*_Mul;

				return col;
			}
			ENDCG
		}
	}

	SubShader
	{
		LOD 100

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMaterial AmbientAndDiffuse
			
			SetTexture [_MainTex]
			{
				Combine Texture * Primary
			}
		}
	}
}
