// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WireFrameReborn" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ThresholdY("Threshold Y", Float) = 20
		_Color("Main Color", Color) = (1,1,1,1)

		_EnvTex ("Base (RGB)", 2D) = "white" {}
		_UVAnimX ("UV Anim X", float) = 0
		_UVAnimY ("UV Anim Y", float) = 0

		_Scale ("Scale", float) = 2
	}
	
	SubShader 
	{	
		Tags { 
			"Queue"="Transparent+5" 
			"IgnoreProjector"="True"
		 }
		pass 
		{
			Cull off
			Blend one  one
			ZTest off
			ZWrite off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform float _ThresholdY;
			uniform float4 _Color;
			sampler2D _MainTex;
			sampler2D _EnvTex;

	        uniform fixed _UVAnimX;
	        uniform fixed _UVAnimY;

	        uniform fixed _Scale;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : TEXCOORD1;
				float2 uv1 : TEXCOORD2;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.color = v.vertex;

				float t1 = _Time.y*_UVAnimX;
				float t2 = _Time.y*_UVAnimY;
				o.uv1 = (v.texcoord+ fixed2(t1, t2))*_Scale;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				if (i.color.z > _ThresholdY)
				{
					discard;
				}

				half4 c = tex2D(_MainTex, i.uv) * _Color;
				//c.rgb *= 2;
				fixed4 envTex = tex2D(_EnvTex, i.uv1);
				c.rgb += envTex.rgb*c.b*2;
				//c.rgb = envTex.rgb;
				return c;
			}

			ENDCG
		}		
	} 
}
