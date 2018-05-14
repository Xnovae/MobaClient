// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "Custom/tankDeadShader" {
	Properties {
		_Decal ("Decal (RGB) ", 2D) = "white" {}
		_OffsetScale ("DecalOffset ", Vector) = (0, 0, 1, 1)
		_DecalCoff("coff", float) = 1

		_BumpMap ("Decal (RGB) ", 2D) = "white" {}
	}
	
	SubShader {
		Tags { 
			"Queue"="Geometry+5" 
			"IgnoreProjector"="True"
		 }

		Pass {
			Name "BASE"
			
			LOD 200
			CGPROGRAM
			
			#pragma vertex vert 
	        #pragma fragment frag
	        #include "UnityCG.cginc"
	        

			struct VertIn {
	        	float4 vertex : POSITION;
	        	float4 texcoord : TEXCOORD0;
	        	float4 color : COLOR;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
	        };
	        struct v2f {
	        	float4 pos : SV_POSITION;
	        	//float2 uv : TEXCOORD0;
	        	float4 colour : TEXCOORD1;
				fixed2 spUV : TEXCOORD2;

				fixed3 I : TEXCOORD3;
				fixed3 TtoW0 : TEXCOORD4;
				fixed3 TtoW1 : TEXCOORD5;
				fixed3 TtoW2 : TEXCOORD6;

			};
	        
	        uniform sampler2D _Decal;
			uniform fixed4 _OffsetScale; 
			uniform fixed _DecalCoff;

			uniform sampler2D _BumpMap;
			//uniform fixed3 _LightDir;
	        
	        v2f vert(VertIn v) 
			{
				v2f o;
				//tangent 空间调整表面顶点凹凸
				//fixed4 modifyObjVex = v.vertex+v.normal*tex2Dlod(_Decal, v.texcoord);
				o.pos = UnityObjectToClipPos(v.vertex);
				//o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				o.colour = v.color;
				//o.colour = v.tangent*0.5+0.5;
				//o.colour = v.normal*0.5+0.5;
				//o.objPos = (v.vertex.xy+_OffsetScale.xy)*_OffsetScale.zw;
				//o.objPos = v.vertex;
				/*
				fixed r = length(v.vertex);
				fixed theta = acos(v.vertex.z/r);
				fixed gama = atan2(v.vertex.y, v.vertex.x);
				*/
				fixed3 absVer = abs(v.normal);
				if(absVer.x > max(absVer.y, absVer.z)) {
					o.spUV = fixed2(v.vertex.yz);
				}else if(absVer.y > absVer.z) {
					o.spUV = fixed2(v.vertex.xz);
				}else {
					o.spUV = fixed2(v.vertex.xy);
				}
				//o.spUV = fixed2(theta, gama);

				o.I = -WorldSpaceViewDir(v.vertex);
				TANGENT_SPACE_ROTATION;
				o.TtoW0 = mul(rotation, unity_ObjectToWorld[0].xyz*1.0);
				o.TtoW1 = mul(rotation, unity_ObjectToWorld[1].xyz*1.0);
				o.TtoW2 = mul(rotation, unity_ObjectToWorld[2].xyz*1.0);
				return o;
			}
			
			fixed4 frag(v2f i) : Color {
				fixed4 col = (i.colour + tex2D(_Decal, i.spUV));//*_DecalCoff;
				//return col;
				//return i.colour;
				
				//fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
				fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.spUV));

				fixed3 wn;
				wn.x = dot(i.TtoW0, normal); 
				wn.y = dot(i.TtoW1, normal); 
				wn.z = dot(i.TtoW2, normal); 

				fixed3 lightDir = normalize(fixed3(1, 1, 1));
				fixed cosV = dot(lightDir, wn);
				cosV = max(cosV, -cosV);
				fixed spec = max(dot(normalize( i.I), reflect(lightDir, wn) ), 0);

				/*
				if(cosV < 0) {
					cosV = -cosV;
				}
				*/
				return (col+pow(spec, 10))*cosV*_DecalCoff;
				//return i.colour;
				//return fixed4(cosV, cosV, cosV, 1);
			}
	        ENDCG
		}

	} 
	FallBack "Diffuse"
}
