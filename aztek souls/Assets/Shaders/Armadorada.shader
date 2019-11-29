// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "t"
{
	Properties
	{
		_axe_DIF2("axe_DIF2", 2D) = "white" {}
		_axe_NM2("axe_NM2", 2D) = "bump" {}
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Color("Color", Color) = (0.5660378,0,0,0)
		_axe_emission("axe_emission", 2D) = "white" {}
		_ChangeValue("ChangeValue", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _axe_NM2;
		uniform float4 _axe_NM2_ST;
		uniform sampler2D _axe_DIF2;
		uniform float4 _axe_DIF2_ST;
		uniform sampler2D _axe_emission;
		uniform float4 _axe_emission_ST;
		uniform float4 _Color;
		uniform float _ChangeValue;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_axe_NM2 = i.uv_texcoord * _axe_NM2_ST.xy + _axe_NM2_ST.zw;
			o.Normal = UnpackNormal( tex2D( _axe_NM2, uv_axe_NM2 ) );
			float2 uv_axe_DIF2 = i.uv_texcoord * _axe_DIF2_ST.xy + _axe_DIF2_ST.zw;
			o.Albedo = tex2D( _axe_DIF2, uv_axe_DIF2 ).rgb;
			float2 uv_axe_emission = i.uv_texcoord * _axe_emission_ST.xy + _axe_emission_ST.zw;
			float4 tex2DNode15 = tex2D( _axe_emission, uv_axe_emission );
			float4 color17 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float4 lerpResult19 = lerp( ( tex2DNode15 * color17 ) , ( tex2DNode15 * _Color ) , _ChangeValue);
			o.Emission = lerpResult19.rgb;
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			o.Metallic = tex2D( _TextureSample0, uv_TextureSample0 ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16700
776;509;865;492;834.3542;-469.2894;1.592149;True;False
Node;AmplifyShaderEditor.SamplerNode;15;-616.2688,580.2062;Float;True;Property;_axe_emission;axe_emission;4;0;Create;True;0;0;False;0;ec1352734d564a84282c1ce6e2f0daf7;ec1352734d564a84282c1ce6e2f0daf7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;17;-455.4826,1036.041;Float;False;Constant;_Color1;Color 1;6;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;10;-543.6836,803.564;Float;False;Property;_Color;Color;3;0;Create;True;0;0;False;0;0.5660378,0,0,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-197.366,632.4624;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-194.3634,824.1898;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-78.14574,1022.015;Float;False;Property;_ChangeValue;ChangeValue;5;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-611.9541,149.4429;Float;True;Property;_axe_NM2;axe_NM2;1;0;Create;True;0;0;False;0;c01078a6446f53c46a6ddf9dbd56b7aa;c01078a6446f53c46a6ddf9dbd56b7aa;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-614.1694,-41.04794;Float;True;Property;_axe_DIF2;axe_DIF2;0;0;Create;True;0;0;False;0;8fba20f0e8a257f40b0372784d3569c9;8fba20f0e8a257f40b0372784d3569c9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;8;-612.0152,360.0718;Float;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;False;0;1960e5a34b400f84486cc584dab34681;1960e5a34b400f84486cc584dab34681;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;19;212.7595,767.7905;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;386.4433,-28.71856;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;t;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;15;0
WireConnection;9;1;10;0
WireConnection;18;0;15;0
WireConnection;18;1;17;0
WireConnection;19;0;18;0
WireConnection;19;1;9;0
WireConnection;19;2;20;0
WireConnection;0;0;2;0
WireConnection;0;1;4;0
WireConnection;0;2;19;0
WireConnection;0;3;8;0
ASEEND*/
//CHKSM=1CFCF80474EC442CD4214DA5F4F311D145E43240