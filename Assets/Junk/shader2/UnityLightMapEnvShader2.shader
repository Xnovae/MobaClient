// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

Shader "Custom/UnityLightMapEnvShader2" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType" = "Opaque" 
			"PreviewType" = "Plane"
		}
		Pass {
			Lighting Off
			CGPROGRAM
			  #pragma vertex vert
			  #pragma fragment frag
			  #include "UnityCG.cginc"



	      struct appdata_lightmap {
	        float4 vertex : POSITION;
	        float2 texcoord : TEXCOORD0;
	        float2 texcoord1 : TEXCOORD1;
	      };

		  //光照贴图需要模型有两套UV
	      struct v2f {
	        float4 pos : SV_POSITION;
	        float2 uv0 : TEXCOORD0;
	        //float2 uv1 : TEXCOORD1;
			fixed3 shadowPos : TEXCOORD2;
			fixed3 offPos : TEXCOORD3;
	      };

	      //unity光照贴图
	      // sampler2D unity_Lightmap;
	      //光照贴图的位置的缩放和偏移
	      // float4 unity_LightmapST;

	      //纹理动画需要ST
	      sampler2D _MainTex;
	      float4 _MainTex_ST; 


		  //阴影相关
			uniform sampler2D _ShadowMap;
		    uniform float4 _ShadowCamPos;
		    uniform float _ShadowCameraSize;

			//光照蒙版 防止光照贴图边缘存在问题
			uniform sampler2D _LightMask;

			//动态光照
			uniform float _LightCoff;
		    uniform float4 _CamPos;
		    uniform float _CameraSize;
			uniform sampler2D _LightMap;
		    uniform float4 _AmbientCol;


	      v2f vert(appdata_lightmap i) {
	        v2f o;
	        o.pos = UnityObjectToClipPos(i.vertex);

	        o.uv0 = TRANSFORM_TEX(i.texcoord, _MainTex);
	        //o.uv1 = i.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
			fixed3 worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
			o.shadowPos = worldPos-(_WorldSpaceCameraPos+_ShadowCamPos);

			o.offPos = worldPos-(_WorldSpaceCameraPos+_CamPos);
	        return o;
	      }

	      half4 frag(v2f i) : COLOR {
	        half4 main_color = tex2D(_MainTex, i.uv0);
	        //main_color.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv1));

			fixed2 mapShadow = (i.shadowPos.xz+float2(_ShadowCameraSize, _ShadowCameraSize))/(2*_ShadowCameraSize);
			fixed4 shadowC = tex2D(_ShadowMap, mapShadow);

			fixed2 mapUV = (i.offPos.xz+float2(_CameraSize, _CameraSize))/(2*_CameraSize);
			fixed3 lightCol = tex2D(_LightMap, mapUV).rgb * _LightCoff;
			 
			fixed mask = 1-tex2D(_LightMask, mapUV).a;

			main_color.rgb = (1-mask)*main_color.rgb+ mask*main_color.rgb * (_AmbientCol.rgb+lightCol);
	        return main_color;
	      }
	      ENDCG
		}
		
	} 
	FallBack "Diffuse"
}
