// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "New Amplify Shader 2"
{
	Properties
	{
		_FogIntensity("FogIntensity", Range( 0 , 1)) = 1
		_FogMaxIntensity("Fog Max Intensity", Range( 0 , 1)) = 0
		_uniformclouds("uniformclouds", 2D) = "white" {}
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform sampler2D _uniformclouds;
		uniform sampler2D _TextureSample0;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _FogIntensity;
		uniform float _FogMaxIntensity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 panner17 = ( 0.05 * _Time.y * float2( 0.2,0.2 ) + i.uv_texcoord);
			float4 color29 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			o.Albedo = saturate( ( tex2D( _uniformclouds, panner17 ) * color29 ) ).rgb;
			float4 normalizeResult33 = normalize( tex2D( _TextureSample0, panner17 ) );
			float4 clampResult22 = clamp( normalizeResult33 , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
			o.Emission = clampResult22.rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float eyeDepth3 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos ))));
			float clampResult10 = clamp( ( abs( ( eyeDepth3 - ase_screenPos.w ) ) * (0.01 + (_FogIntensity - 0.0) * (0.4 - 0.01) / (1.0 - 0.0)) ) , 0.0 , _FogMaxIntensity );
			o.Alpha = clampResult10;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16700
0;508;1529;493;1855.684;151.4608;1.840653;True;False
Node;AmplifyShaderEditor.ScreenPosInputsNode;2;-1291.515,97.21272;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-904.4332,-322.969;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenDepthNode;3;-949.7199,52.51871;Float;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;4;-660.5294,125.9348;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;17;-623.6382,-246.3017;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.2,0.2;False;1;FLOAT;0.05;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-879.3584,409.3629;Float;True;Property;_FogIntensity;FogIntensity;0;0;Create;True;0;0;False;0;1;0.128;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;6;-433.1715,124.8386;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;20;-235.5653,-60.17741;Float;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;False;0;1e73649a67c8dfe439d93befbfe7c475;df04cfad6faff634ebfb78d3c95185d3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;29;105.5984,-179.4822;Float;False;Constant;_Color0;Color 0;4;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;15;-158.7986,-392.9239;Float;True;Property;_uniformclouds;uniformclouds;2;0;Create;True;0;0;False;0;1e73649a67c8dfe439d93befbfe7c475;1e73649a67c8dfe439d93befbfe7c475;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;8;-496.7584,351.1649;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.01;False;4;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;33;121.178,76.78027;Float;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-529.468,668.5533;Float;False;Property;_FogMaxIntensity;Fog Max Intensity;1;0;Create;True;0;0;False;0;0;0.781;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;384.3914,-379.7018;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-316.7752,139.9268;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;16;450.0462,-159.3301;Float;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;22;427.756,69.82321;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;10;-23.34326,411.3418;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;899.4133,-70.37811;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;New Amplify Shader 2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;2;0
WireConnection;4;0;3;0
WireConnection;4;1;2;4
WireConnection;17;0;18;0
WireConnection;6;0;4;0
WireConnection;20;1;17;0
WireConnection;15;1;17;0
WireConnection;8;0;7;0
WireConnection;33;0;20;0
WireConnection;31;0;15;0
WireConnection;31;1;29;0
WireConnection;5;0;6;0
WireConnection;5;1;8;0
WireConnection;16;0;31;0
WireConnection;22;0;33;0
WireConnection;10;0;5;0
WireConnection;10;2;9;0
WireConnection;0;0;16;0
WireConnection;0;2;22;0
WireConnection;0;9;10;0
ASEEND*/
//CHKSM=EA076490D7A5E94A1A3DB0BA16E79307F9C1CA3D