// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.05 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.05;sub:START;pass:START;ps:flbk:,lico:0,lgpr:1,nrmq:0,limd:0,uamb:True,mssp:True,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:2,bsrc:0,bdst:0,culm:2,dpts:6,wrdp:False,dith:0,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.1280277,fgcg:0.1953466,fgcb:0.2352941,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:3966,x:32811,y:32678,varname:node_3966,prsc:2|diff-2169-OUT,emission-2169-OUT;n:type:ShaderForge.SFN_Tex2d,id:185,x:31471,y:32683,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_185,prsc:2,tex:98ecf7b27219e354cb062f08e73e8a72,ntxv:0,isnm:False|UVIN-4372-UVOUT;n:type:ShaderForge.SFN_Add,id:544,x:31842,y:32687,varname:node_544,prsc:2|A-185-RGB,B-4544-OUT;n:type:ShaderForge.SFN_Multiply,id:4544,x:31682,y:32755,varname:node_4544,prsc:2|A-185-RGB,B-2534-OUT;n:type:ShaderForge.SFN_Vector1,id:2534,x:31471,y:32880,varname:node_2534,prsc:2,v1:0.7;n:type:ShaderForge.SFN_Multiply,id:4145,x:32117,y:32694,varname:node_4145,prsc:2|A-544-OUT,B-2604-RGB;n:type:ShaderForge.SFN_Tex2d,id:2604,x:31887,y:32854,ptovrint:False,ptlb:MaskTex,ptin:_MaskTex,varname:node_2604,prsc:2,tex:40588ed4b9bc45b43b9260349cfd8583,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Time,id:7161,x:30594,y:32789,varname:node_7161,prsc:2;n:type:ShaderForge.SFN_Panner,id:4372,x:31095,y:32678,varname:node_4372,prsc:2,spu:-1,spv:3|UVIN-2670-UVOUT,DIST-4051-OUT;n:type:ShaderForge.SFN_Multiply,id:4051,x:30837,y:32800,varname:node_4051,prsc:2|A-7161-TSL,B-6079-OUT;n:type:ShaderForge.SFN_Vector1,id:6079,x:30594,y:32966,varname:node_6079,prsc:2,v1:4;n:type:ShaderForge.SFN_TexCoord,id:2670,x:30837,y:32585,varname:node_2670,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:2169,x:32420,y:32690,varname:node_2169,prsc:2|A-4145-OUT,B-4455-RGB;n:type:ShaderForge.SFN_Color,id:4455,x:32167,y:32868,ptovrint:False,ptlb:TintColor,ptin:_TintColor,varname:node_4455,prsc:2,glob:False,c1:0.5,c2:0.5,c3:0.5,c4:1;proporder:185-2604-4455;pass:END;sub:END;*/

Shader "Custom/FlowLight" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _MaskTex ("MaskTex", 2D) = "white" {}
        _TintColor ("TintColor", Color) = (0.5,0.5,0.5,1)
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            Cull Off
            ZTest Always
            ZWrite Off
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _MaskTex; uniform float4 _MaskTex_ST;
            uniform float4 _TintColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
/////// Vectors:
////// Lighting:
////// Emissive:
                float4 node_7161 = _Time + _TimeEditor;
                float2 node_4372 = (i.uv0+(node_7161.r*4.0)*float2(-1,3));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_4372, _MainTex));
                float4 _MaskTex_var = tex2D(_MaskTex,TRANSFORM_TEX(i.uv0, _MaskTex));
                float3 node_2169 = (((_MainTex_var.rgb+(_MainTex_var.rgb*0.7))*_MaskTex_var.rgb)*_TintColor.rgb);
                float3 emissive = node_2169;
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
