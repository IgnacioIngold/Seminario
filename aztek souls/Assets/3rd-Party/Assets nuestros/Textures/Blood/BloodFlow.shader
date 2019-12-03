// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Blood / Flow"
{
	Properties
	{
		_Blood_flow_sanja_AlbedoTransparency("Blood_flow_sanja_AlbedoTransparency", 2D) = "white" {}
		_Blood_flow_sanja_MetallicSmoothness("Blood_flow_sanja_MetallicSmoothness", 2D) = "white" {}
		_Blood_flow_sanja_Normal("Blood_flow_sanja_Normal", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
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

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 panner5 = ( 1.0 * _Time.y * float2( 0,0.05 ) + i.uv_texcoord);
			o.Normal = UnpackNormal( tex2D( _Blood_flow_sanja_Normal, panner5 ) );
			o.Albedo = tex2D( _Blood_flow_sanja_AlbedoTransparency, panner5 ).rgb;
			o.Metallic = tex2D( _Blood_flow_sanja_MetallicSmoothness, panner5 ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16700
536;489;922;512;1539.103;223.311;2.052452;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-1764.873,-237.678;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;5;-1370.803,-198.6814;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0.05;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;4;-909.9139,124.3509;Float;True;Property;_Blood_flow_sanja_Normal;Blood_flow_sanja_Normal;3;0;Create;True;0;0;False;0;afad6a9bc64adce4ca41d1df55318073;afad6a9bc64adce4ca41d1df55318073;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-934.6031,-85.8672;Float;True;Property;_Blood_flow_sanja_MetallicSmoothness;Blood_flow_sanja_MetallicSmoothness;2;0;Create;True;0;0;False;0;8db9cd2bd5558be4bbbd86dd7df1f0dd;8db9cd2bd5558be4bbbd86dd7df1f0dd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-910.1807,-336.9473;Float;True;Property;_Blood_flow_sanja_AlbedoTransparency;Blood_flow_sanja_AlbedoTransparency;0;0;Create;True;0;0;False;0;0d69628525ff4974cae1197b38238067;0d69628525ff4974cae1197b38238067;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-588.7252,375.0656;Float;True;Property;_Blood_flow_sanja_Opacity;Blood_flow_sanja_Opacity;1;0;Create;True;0;0;False;0;e69bab758db746241ac5765b74702a65;e69bab758db746241ac5765b74702a65;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Blood / Flow;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;6;0
WireConnection;4;1;5;0
WireConnection;3;1;5;0
WireConnection;1;1;5;0
WireConnection;0;0;1;0
WireConnection;0;1;4;0
WireConnection;0;3;3;0
ASEEND*/
//CHKSM=D1B192C3D30B0EA3846889343BD785268AFC0ED7