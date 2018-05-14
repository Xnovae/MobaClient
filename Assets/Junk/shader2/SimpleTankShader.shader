// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SimpleTankShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainColor ("Tint Color", Color) = (1,1,1,1)
		_Alpha("Alpha", float) = 1
	}
	SubShader {
		Tags { 
			"Queue"="Transparent+5" 
			"IgnoreProjector"="True"
		 }

		Pass {
			Name "BASE"
			Blend SrcAlpha OneMinusSrcAlpha
			
			LOD 200
			CGPROGRAM
			
			#pragma vertex vert 
	        #pragma fragment frag
	        #include "UnityCG.cginc"
	        

			struct VertIn {
	        	float4 vertex : POSITION;
	        	float4 texcoord : TEXCOORD0;
	        };
	        struct v2f {
	        	float4 pos : SV_POSITION;
	        	float2 uv : TEXCOORD0;
	        };
	        
	        uniform sampler2D _MainTex;
			uniform fixed4 _MainColor; 
			uniform fixed _Alpha;
	        
	        v2f vert(VertIn v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o;
			}
			
			fixed4 frag(v2f i) : Color {
				
				return tex2D(_MainTex, i.uv)*fixed4(_MainColor.rgb, _Alpha);
			}
	        ENDCG
		}

	} 
	FallBack "Diffuse"
}
