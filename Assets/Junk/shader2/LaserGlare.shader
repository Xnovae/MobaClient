// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/LaserGlare" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Tags { "Queue"="Transparent+8" 
				"IgnoreProjector"="True"
				"RenderType"="Transparent" }
		
		Pass {
			LOD 200
			Blend One One  
			zwrite off

			CGPROGRAM
			#pragma vertex vert 
	        #pragma fragment frag
	        #include "UnityCG.cginc"
			struct VertIn {
	        	float4 vertex : POSITION;
	        	float4 texcoord : TEXCOORD0;
	        };
	        
	        struct v2f {
	        	fixed4 pos : SV_POSITION;
	        	fixed2 uv : TEXCOORD0;
	        };

	        uniform sampler2D _MainTex; 
			uniform fixed4 _Color;

	         
	        v2f vert(VertIn v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o;
			}

			fixed4 frag(v2f i) : Color {
                fixed4 tex2 = tex2D(_MainTex, i.uv);
                fixed4 col;
                col.rgb = tex2.rgb * _Color.rgb;
                col.a = tex2.a * _Color.a;
				return col;
			}
			
			ENDCG
		}
	
	} 
	FallBack "Diffuse"
}
