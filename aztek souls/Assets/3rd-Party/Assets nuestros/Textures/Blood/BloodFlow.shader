// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Blood / flow blood"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Blood_flow_sanja_AlbedoTransparency("Blood_flow_sanja_AlbedoTransparency", 2D) = "white" {}
		_Blood_flow_sanja_MetallicSmoothness("Blood_flow_sanja_MetallicSmoothness", 2D) = "white" {}
		_Blood_flow_sanja_Normal("Blood_flow_sanja_Normal", 2D) = "bump" {}
		_Blood_flow_sanja_Opacity("Blood_flow_sanja_Opacity", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Blood_flow_sanja_Normal;
		uniform sampler2D _Blood_flow_sanja_AlbedoTransparency;
		uniform sampler2D _Blood_flow_sanja_MetallicSmoothness;
		uniform sampler2D _Blood_flow_sanja_Opacity;
		uniform float4 _Blood_flow_sanja_Opacity_ST;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 panner5 = ( 1.0 * _Time.y * float2( 0,0.05 ) + i.uv_texcoord);
			o.Normal = UnpackNormal( tex2D( _Blood_flow_sanja_Normal, panner5 ) );
			o.Albedo = tex2D( _Blood_flow_sanja_AlbedoTransparency, panner5 ).rgb;
			o.Metallic = tex2D( _Blood_flow_sanja_MetallicSmoothness, panner5 ).r;
			o.Alpha = 1;
			float2 uv_Blood_flow_sanja_Opacity = i.uv_texcoord * _Blood_flow_sanja_Opacity_ST.xy + _Blood_flow_sanja_Opacity_ST.zw;
			clip( tex2D( _Blood_flow_sanja_Opacity, uv_Blood_flow_sanja_Opacity ).r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16700
0;728;1529;273;1737.968;-267.2318;1.973638;False;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-2033.066,-296.737;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;5;-1504.962,-176.9518;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0.05;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;4;-483.66,636.0889;Float;True;Property;_Blood_flow_sanja_Opacity;Blood_flow_sanja_Opacity;4;0;Create;True;0;0;False;0;e69bab758db746241ac5765b74702a65;e69bab758db746241ac5765b74702a65;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1153.22,-227.7637;Float;True;Property;_Blood_flow_sanja_AlbedoTransparency;Blood_flow_sanja_AlbedoTransparency;1;0;Create;True;0;0;False;0;0d69628525ff4974cae1197b38238067;0d69628525ff4974cae1197b38238067;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-1095.654,-11.28168;Float;True;Property;_Blood_flow_sanja_Normal;Blood_flow_sanja_Normal;3;0;Create;True;0;0;False;0;afad6a9bc64adce4ca41d1df55318073;afad6a9bc64adce4ca41d1df55318073;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-1103.976,206.3644;Float;True;Property;_Blood_flow_sanja_MetallicSmoothness;Blood_flow_sanja_MetallicSmoothness;2;0;Create;True;0;0;False;0;8db9cd2bd5558be4bbbd86dd7df1f0dd;8db9cd2bd5558be4bbbd86dd7df1f0dd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;10.12421,131.6148;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Blood / flow blood;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Transparent;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;7;0
WireConnection;1;1;5;0
WireConnection;3;1;5;0
WireConnection;2;1;5;0
WireConnection;0;0;1;0
WireConnection;0;1;3;0
WireConnection;0;3;2;0
WireConnection;0;10;4;0
ASEEND*/
//CHKSM=376CE449A5AF455D76831AABFA8B2F3F80840876