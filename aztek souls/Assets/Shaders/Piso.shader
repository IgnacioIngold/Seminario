// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Piso"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_TextureSample1("Texture Sample 1", 2D) = "bump" {}
		_Tile("Tile", Float) = 9
		_TextureSample2("Texture Sample 2", 2D) = "white" {}
		_TextureSample3("Texture Sample 3", 2D) = "white" {}
		_NormalIntensity("Normal Intensity", Range( 0 , 1)) = 0
		_TextureSample4("Texture Sample 4", 2D) = "white" {}
		_Tiledecals("Tile decals", Float) = 16.16
		_Smoth("Smoth", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float _NormalIntensity;
		uniform sampler2D _TextureSample1;
		uniform float _Tile;
		uniform sampler2D _TextureSample0;
		uniform sampler2D _TextureSample4;
		uniform float _Tiledecals;
		uniform sampler2D _TextureSample2;
		uniform float _Smoth;
		uniform sampler2D _TextureSample3;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float2 temp_output_65_0 = (( ase_worldPos / _Tile )).xz;
			o.Normal = UnpackScaleNormal( tex2D( _TextureSample1, temp_output_65_0 ), _NormalIntensity );
			float4 tex2DNode78 = tex2D( _TextureSample4, (( ase_worldPos / _Tiledecals )).xz );
			float4 color80 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 lerpResult76 = lerp( tex2D( _TextureSample0, temp_output_65_0 ) , ( tex2DNode78.r * color80 ) , tex2DNode78.a);
			o.Albedo = lerpResult76.rgb;
			o.Metallic = tex2D( _TextureSample2, temp_output_65_0 ).r;
			o.Smoothness = _Smoth;
			o.Occlusion = tex2D( _TextureSample3, temp_output_65_0 ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16700
202;728;1245;273;974.2305;-33.4418;1;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;46;-1741.153,-310.4336;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;84;-1657.675,328.001;Float;False;Property;_Tiledecals;Tile decals;7;0;Create;True;0;0;False;0;16.16;18.23;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;81;-1450.77,267.5701;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-1668.872,28.66791;Float;False;Property;_Tile;Tile;2;0;Create;True;0;0;False;0;9;9;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;67;-1377.077,-116.3967;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;82;-1265.361,264.7559;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;78;-598.6636,600.0695;Float;True;Property;_TextureSample4;Texture Sample 4;6;0;Create;True;0;0;False;0;None;593d1600e8967a84484db7d4d600892b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;80;-436.8146,830.6484;Float;False;Constant;_Color0;Color 0;8;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;65;-1191.668,-118.0619;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-175.026,579.3751;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-1000.863,89.38144;Float;False;Property;_NormalIntensity;Normal Intensity;5;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-607.5938,-203.8646;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;98ad531f14beee24cbed4d1baeb72af6;98ad531f14beee24cbed4d1baeb72af6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;68;-604.1759,-3.355781;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;None;003d347a6e6702948a36d8169fdf7240;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;74;-138.4796,117.8788;Float;False;Property;_Smoth;Smoth;8;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;70;-587.7371,390.5574;Float;True;Property;_TextureSample3;Texture Sample 3;4;0;Create;True;0;0;False;0;None;e5082d97b61402840aa297417db1e724;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;76;-121.1151,-158.2686;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;69;-609.2391,183.6852;Float;True;Property;_TextureSample2;Texture Sample 2;3;0;Create;True;0;0;False;0;None;9a0b0e45d88b51245a1fd275fd3fbfcd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;78,-24;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Piso;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;81;0;46;0
WireConnection;81;1;84;0
WireConnection;67;0;46;0
WireConnection;67;1;66;0
WireConnection;82;0;81;0
WireConnection;78;1;82;0
WireConnection;65;0;67;0
WireConnection;79;0;78;1
WireConnection;79;1;80;0
WireConnection;2;1;65;0
WireConnection;68;1;65;0
WireConnection;68;5;72;0
WireConnection;70;1;65;0
WireConnection;76;0;2;0
WireConnection;76;1;79;0
WireConnection;76;2;78;4
WireConnection;69;1;65;0
WireConnection;0;0;76;0
WireConnection;0;1;68;0
WireConnection;0;3;69;0
WireConnection;0;4;74;0
WireConnection;0;5;70;0
ASEEND*/
//CHKSM=B4316E3F0DEFB1BAA992B867D5961FC53604F3CE