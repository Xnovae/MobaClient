// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/tankPlayerShader4" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_NormTex ("NormTex (RGB)", 2D) = "white" {}
		//_SpecTex ("SpecTex (RGB)", 2D) = "white" {}
		//_IllumTex ("Base (RGB)", 2D) = "white" {}
	}
	
	SubShader {
		Tags { 
			"Queue"="Geometry+5" 
			"IgnoreProjector"="True"
		 }

		Pass {
			Name "BASE"
	        Lighting off
			
			LOD 200
			CGPROGRAM
			
			#pragma vertex vert 
	        #pragma fragment frag
	        #include "UnityCG.cginc"
	        
			//uniform fixed4 _HighLightDir;
			//uniform fixed4 _Ambient;
			//uniform fixed4 _LightDiffuseColor;
			//uniform fixed3 _LightDir;

			struct VertIn {
	        	float4 vertex : POSITION;
	        	float4 texcoord : TEXCOORD0;
	        	float3 normal : NORMAL;
				float4 tangent : TANGENT;
	        };
	        struct v2f {
	        	float4 pos : SV_POSITION;
	        	float2 uv : TEXCOORD0;

				float4 posWorld : TEXCOORD1;
				float3 tangentWorld : TEXCOORD2;
				float3 normalWorld : TEXCOORD3;
				float3 binormalWorld : TEXCOORD4;

	        };
	        
	        uniform sampler2D _MainTex;
	        uniform sampler2D _NormTex;
	        //uniform sampler2D _SpecTex;
	        //uniform sampler2D _IllumTex;
			//uniform fixed3 _AmbientCol;
			uniform fixed3 _SpecColor;
			uniform fixed _Shinness;
	        
	        v2f vert(VertIn v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);

				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;

				o.posWorld = mul(modelMatrix, v.vertex);
				
				float3 tangentWorld = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
				o.tangentWorld = tangentWorld;
				float3 normalWorld = normalize(mul(float4(v.normal, 0), modelMatrixInverse).xyz);
				o.normalWorld = normalWorld;

				o.binormalWorld = normalize(cross(normalWorld, tangentWorld) * v.tangent.w);

				return o;
			}
			
			fixed4 frag(v2f i) : Color {
				const fixed3 lightDir = normalize(fixed3(1, 1, 1));
				fixed3 normal = UnpackNormal(tex2D(_NormTex, i.uv));

				float3x3 local2WorldTranspose = float3x3(
					i.tangentWorld,
					i.binormalWorld,
					i.normalWorld
				);
				float3 normalDirection = normalize(mul(normal, local2WorldTranspose));

				float diffCoff = max(0.0, dot(normalDirection, lightDir));

				//fixed4 specColor = tex2D(_SpecTex, i.uv);

				fixed3 viewDirection = normalize(_WorldSpaceCameraPos-i.posWorld.xyz);
				/*
				fixed specRef = 0;
				if(dot(normalDirection, lightDir) < 0.0) {
					
				}else {
					specRef = max(0.0, 
						pow(max(0.0, dot(reflect(-lightDir, normalDirection), viewDirection)), _Shinness * specColor.r) 
					);
				}
				*/
				//fixed4 ilum = tex2D(_IllumTex, i.uv);

				const float amColor = 0.5;

				fixed4 retCol = tex2D(_MainTex, i.uv)*(diffCoff+amColor);
				//retCol.rgb += _SpecColor*specRef;
				//retCol.rgb += ilum;
				return  retCol;
				//return specColor;
				//return tex2D(_MainTex, i.uv) * specRef;
				//return fixed4(1, 1, 1, 1) * diffCoff;
				//return fixed4(_SpecColor, 1) * specRef;
				//return tex2D(_MainTex, i.uv) * diffCoff;
				//return i.colour;
			}
	        ENDCG
		}

	} 
	FallBack "Diffuse"
}
