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
		_TextureSample5("Texture Sample 5", 2D) = "bump" {}
		_TextureSample6("Texture Sample 6", 2D) = "white" {}
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
		uniform sampler2D _TextureSample5;
		uniform float _Tiledecals;
		uniform sampler2D _TextureSample0;
		uniform sampler2D _TextureSample6;
		uniform sampler2D _TextureSample4;
		uniform sampler2D _TextureSample2;
		uniform float _Smoth;
		uniform sampler2D _TextureSample3;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float2 temp_output_65_0 = (( ase_worldPos / _Tile )).xz;
			float2 temp_output_82_0 = (( ase_worldPos / _Tiledecals )).xz;
			o.Normal = BlendNormals( UnpackScaleNormal( tex2D( _TextureSample1, temp_output_65_0 ), _NormalIntensity ) , UnpackScaleNormal( tex2D( _TextureSample5, temp_output_82_0 ), _NormalIntensity ) );
			float4 tex2DNode87 = tex2D( _TextureSample6, temp_output_65_0 );
			float4 color90 = IsGammaSpace() ? float4(0.07716268,0.3207547,0.088677,0) : float4(0.006836838,0.08393918,0.008354558,0);
			float4 lerpResult88 = lerp( tex2D( _TextureSample0, temp_output_65_0 ) , ( tex2DNode87.r * color90 ) , tex2DNode87.r);
			float4 tex2DNode78 = tex2D( _TextureSample4, temp_output_82_0 );
			float4 color80 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 lerpResult76 = lerp( lerpResult88 , ( tex2DNode78.r * color80 ) , tex2DNode78.a);
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
407;614;937;387;1414.44;672.8716;1.827808;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;46;-1741.153,-310.4336;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;66;-1668.872,28.66791;Float;False;Property;_Tile;Tile;2;0;Create;True;0;0;False;0;9;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-1657.675,328.001;Float;False;Property;_Tiledecals;Tile decals;7;0;Create;True;0;0;False;0;16.16;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;67;-1377.077,-116.3967;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;81;-1450.77,267.5701;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;65;-1163.692,-178.3173;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;90;-658.6805,-405.731;Float;False;Constant;_Color1;Color 1;11;0;Create;True;0;0;False;0;0.07716268,0.3207547,0.088677,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;87;-724.2549,-685.4231;Float;True;Property;_TextureSample6;Texture Sample 6;10;0;Create;True;0;0;False;0;None;0fc61d9a5bc13c9489e420613d4cadde;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;82;-1265.361,264.7559;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-1000.863,89.38144;Float;False;Property;_NormalIntensity;Normal Intensity;5;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;80;-679.3079,1240.519;Float;False;Constant;_Color0;Color 0;8;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-655.7552,-257.3772;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;98ad531f14beee24cbed4d1baeb72af6;98ad531f14beee24cbed4d1baeb72af6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-218.1193,-545.1271;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;78;-692.6299,908.659;Float;True;Property;_TextureSample4;Texture Sample 4;6;0;Create;True;0;0;False;0;None;593d1600e8967a84484db7d4d600892b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;85;-682.6353,216.5397;Float;True;Property;_TextureSample5;Texture Sample 5;9;0;Create;True;0;0;False;0;None;2786ef25dea7b9c4895cd0fbfb874daf;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-202.3065,888.5544;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;88;-181.9796,-307.6372;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;68;-673.1934,-4.999061;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;None;003d347a6e6702948a36d8169fdf7240;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendNormalsNode;86;-302.29,23.87381;Float;True;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;76;-121.1151,-158.2686;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-138.4796,117.8788;Float;False;Property;_Smoth;Smoth;8;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;70;-681.7035,699.1468;Float;True;Property;_TextureSample3;Texture Sample 3;4;0;Create;True;0;0;False;0;None;e5082d97b61402840aa297417db1e724;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;69;-703.2054,492.2748;Float;True;Property;_TextureSample2;Texture Sample 2;3;0;Create;True;0;0;False;0;None;9a0b0e45d88b51245a1fd275fd3fbfcd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;78,-24;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Piso;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;67;0;46;0
WireConnection;67;1;66;0
WireConnection;81;0;46;0
WireConnection;81;1;84;0
WireConnection;65;0;67;0
WireConnection;87;1;65;0
WireConnection;82;0;81;0
WireConnection;2;1;65;0
WireConnection;89;0;87;1
WireConnection;89;1;90;0
WireConnection;78;1;82;0
WireConnection;85;1;82;0
WireConnection;85;5;72;0
WireConnection;79;0;78;1
WireConnection;79;1;80;0
WireConnection;88;0;2;0
WireConnection;88;1;89;0
WireConnection;88;2;87;1
WireConnection;68;1;65;0
WireConnection;68;5;72;0
WireConnection;86;0;68;0
WireConnection;86;1;85;0
WireConnection;76;0;88;0
WireConnection;76;1;79;0
WireConnection;76;2;78;4
WireConnection;70;1;65;0
WireConnection;69;1;65;0
WireConnection;0;0;76;0
WireConnection;0;1;86;0
WireConnection;0;3;69;0
WireConnection;0;4;74;0
WireConnection;0;5;70;0
ASEEND*/
//CHKSM=2DC75AA7DC8145D28494688B36A66F8D66534665