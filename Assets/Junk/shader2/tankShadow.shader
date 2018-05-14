// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/tankShadow" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Tint Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { 
			"Queue" = "Geometry+1" 
			"PreviewType" = "Plane"
		}
		LOD 200
		
		Pass {
			Name "BASE"
			Blend SrcAlpha OneMinusSrcAlpha 
			ztest off
			zwrite off
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
			uniform fixed4 _Color; 
	        
	        v2f vert(VertIn v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				return o;
			}
			
			fixed4 frag(v2f i) : Color {
				fixed a = tex2D(_MainTex, i.uv).r;
				fixed4 col = fixed4(1, 1, 1, a)*_Color;
				return col;
			}
	        ENDCG
		}
	} 
	FallBack "Diffuse"
}
